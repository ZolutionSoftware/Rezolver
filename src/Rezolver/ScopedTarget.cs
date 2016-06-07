using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// A target that produces a single instance of an object within a lifetime scope.
    /// </summary>
    /// <remarks>On its own, this target doesn't do anything - it's designed to wrap another target such that
    /// the code generated from the expression it produces is executed only once for each lifetime scope.
    /// 
    /// Outside of that, the target generates wrapper code that forcibly caches the instance that is produced (whether
    /// it's IDiposable or not) into the current scope's cache (using <see cref="IScopedContainer.AddToScope(object, RezolveContext)"/>)
    /// and retrieves previous instances from that scope (using <see cref="IScopedContainer.GetFromScope(RezolveContext)"/>.</remarks>
    public class ScopedTarget : TargetBase
    {
        private ConstructorInfo _argExceptionCtor
            = MethodCallExtractor.ExtractConstructorCall(() => new ArgumentException("", ""));

        private ITarget _innerTarget;

        public override Type DeclaredType
        {
            get { return _innerTarget.DeclaredType; }
        }

        protected override bool SuppressScopeTracking
        {
            get
            {
                return true;
            }
        }

        public ScopedTarget(ITarget innerTarget)
        {
            innerTarget.MustNotBeNull("innerTarget");
            _innerTarget = innerTarget;
        }

        protected override Expression CreateExpressionBase(CompileContext context)
        {
            //if the lifetime scope is null then throw an exception, because the lack of a scope 
            //prevents the scoped object from doing what it's supposed to do.
            var isScopeNull = Expression.Equal(context.ContextScopePropertyExpression, Expression.Default(typeof(IScopedContainer)));
            var throwArgException = Expression.Throw(Expression.New(_argExceptionCtor,
                Expression.Property(null, typeof(ExceptionResources), "ScopedSingletonRequiresAScope"),
                Expression.Constant(context.RezolveContextParameter.Name ?? "context")));

            var actualType = context.TargetType ?? DeclaredType;

            var lambdaBody = ExpressionHelper.GetLambdaBodyForTarget(_innerTarget,
                new CompileContext(context, actualType, inheritSharedExpressions: true, suppressScopeTracking: true));

            return Expression.Block(
                Expression.IfThen(isScopeNull, throwArgException),
                ExpressionHelper.Make_Scope_GetOrAddCallExpression(context,
                    actualType,
                    Expression.Lambda(lambdaBody, context.RezolveContextParameter),
                    Expression.Constant(false))
                );
        }

        public override bool SupportsType(Type type)
        {
            return _innerTarget.SupportsType(type);
        }
    }

    /// <summary>
    /// Extension method(s) to convert targets into scoped targets.
    /// </summary>
    public static class IRezolveTargetScopingExtensions
    {
        /// <summary>
        /// Creates a <see cref="ScopedTarget"/> from the target on which this method is invoked.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ScopedTarget Scoped(this ITarget target)
        {
            target.MustNotBeNull(nameof(target));
            return new ScopedTarget(target);
        }
    }
}
