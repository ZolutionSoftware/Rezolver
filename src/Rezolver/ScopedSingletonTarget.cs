using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// A target that produces a single instance of an object within a scope or its parent scopes.
	/// 
	/// E.g. if you have an application, such as a website, that creates multiple top-level scopes, within
	/// each of which you wish to have separate single instances of a given object (such as a database connection),
	/// then the ScopeSingletonTarget is for you.
	/// </summary>
    /// <remarks>Any codde produced by this target, or a class inheriting from this
    /// target, is expected to operate on the current scope directly, rather than relying on it
    /// to track objects for it.</remarks>
	public class ScopedSingletonTarget : RezolveTargetBase
	{
		private MethodInfo _scopeGetSingle
			= MethodCallExtractor.ExtractCalledMethod((ILifetimeScopeRezolver r) => r.GetSingleFromScope(null, false));

		private MethodInfo _addToScope
			= MethodCallExtractor.ExtractCalledMethod((ILifetimeScopeRezolver r) => r.AddToScope(null, null));

		private ConstructorInfo _argExceptionCtor
			= MethodCallExtractor.ExtractConstructorCall(() => new ArgumentException("", ""));

		private IRezolveTarget _innerTarget;

		public override Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}

		public ScopedSingletonTarget(IRezolveTarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			_innerTarget = innerTarget;
		}

		protected override System.Linq.Expressions.Expression CreateExpressionBase(CompileContext context)
		{
			//if the lifetime scope is null then throw an exception, because the lack of a scope 
			//prevents the singleton from doing what it's supposed to do.
			var isScopeNull = Expression.Equal(context.ContextScopePropertyExpression, Expression.Default(typeof(ILifetimeScopeRezolver)));
			var throwArgException = Expression.Throw(Expression.New(_argExceptionCtor, 
				Expression.Property(null, typeof(Resources.Exceptions), "ScopedSingletonRequiresAScope"),
				Expression.Constant(context.RezolveContextParameter.Name ?? "context")));

			var actualType = context.TargetType ?? DeclaredType;
			var localVar = Expression.Parameter(typeof(object), "fromScope");
			var toReturnVar = Expression.Parameter(actualType, "toReturn");
			var getObjectFromScope = Expression.Call(null, _scopeGetSingle, context.ContextScopePropertyExpression, context.RezolveContextParameter, Expression.Constant(true));
			var assignToFromScope = Expression.Assign(localVar, getObjectFromScope);
			//used to search the scope and its parent scopes for an instance of an object that satisfies the rezolve context.
			var convertFromScope = Expression.Convert(localVar, actualType);

            var staticExpr = _innerTarget.CreateExpression(new CompileContext(context, context.TargetType, inheritSharedExpressions: true, suppressScopeTrackingExpressions: true));
			if(staticExpr.Type != actualType)
				staticExpr = Expression.Convert(staticExpr, actualType);
            
			return Expression.Block(actualType,
 				new[] { localVar, toReturnVar },
				Expression.IfThen(isScopeNull, throwArgException),
				assignToFromScope,
				Expression.IfThenElse(Expression.NotEqual(localVar, Expression.Default(typeof(object))), 
					Expression.Assign(toReturnVar, Expression.Convert(localVar, actualType)),
					Expression.Block(
						Expression.Assign(toReturnVar, staticExpr),
						Expression.IfThen(
							Expression.Not(Expression.TypeIs(toReturnVar, typeof(IDisposable))),
							Expression.Call(context.ContextScopePropertyExpression, _addToScope, toReturnVar, context.RezolveContextParameter)
						)
					)
				),
				toReturnVar
			);
		}

        protected override Expression CreateScopeTrackingExpression(CompileContext context, Expression expression)
        {
            //overrides this to prevent the base scope tracking expression being generated - it's not needed because
            //the code produced by this target does itss own scope tracking.
            return expression;
        }

        public override bool SupportsType(Type type)
		{
			return _innerTarget.SupportsType(type);
		}
	}
}
