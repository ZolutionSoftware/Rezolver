using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Resources;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// Default implementation of the IRezolveTargetAdapter interface.
		/// 
		/// Also an ExpressionVisitor.
    /// 
    /// Also, its <see cref="Default" /> property serves as the reference to the default adapter used by the 
    /// system to convert expressions into IRezolveTarget instances.
    /// 
    /// This class cannot be created directly - it is a singleton accessed through the <see cref="Instance" />
    /// property.  You can inherit from this class, however, to serve as the basis for your own implementation
    /// of <see cref="IRezolveTargetAdapter"/>.
    /// </summary>
    public class RezolveTargetAdapter : ExpressionVisitor, IRezolveTargetAdapter
    {
        internal class RezolveCallExpressionInfo
        {
            public Type Type { get; private set; }
            public IRezolveTarget Name { get; private set; }

            internal RezolveCallExpressionInfo(Type type, IRezolveTarget name)
            {
                Type = type;
                Name = name;
            }
        }

        internal static readonly MethodInfo[] RezolveMethods =
        {
            MethodCallExtractor.ExtractCalledMethod((RezolveContextExpressionHelper builder) => builder.Resolve<int>()).GetGenericMethodDefinition()
            ,
            MethodCallExtractor.ExtractCalledMethod((RezolveContextExpressionHelper builder) => builder.Resolve<int>(null))
                .GetGenericMethodDefinition()
        };

        /// <summary>
        /// a lazy that creates one instance of this class to be used as the ultimate default 
        /// </summary>
        private static readonly Lazy<IRezolveTargetAdapter> _instance =
            new Lazy<IRezolveTargetAdapter>(() => new RezolveTargetAdapter());

        /// <summary>
        /// The one and only instance of the RezolveTargetAdapter class
        /// </summary>
        public static IRezolveTargetAdapter Instance
        {
            get { return _instance.Value; }
        }

        private static IRezolveTargetAdapter _default = _instance.Value;

        /// <summary>
        /// The default IRezolveTargetAdapter to be used in converting expressions to IRezolveTarget instances.
        /// By default, this is initialised to a single instance of the <see cref="RezolveTargetAdapter"/> class.
        /// </summary>
        public static IRezolveTargetAdapter Default
        {
            get { return _default; }
            set
            {
                value.MustNotBeNull("value");

                _default = value;
            }
        }

        /// <summary>
        /// Protected constructor ensuring that new instances can only be created through inheritance.
        /// </summary>
        protected RezolveTargetAdapter()
        {

        }

        internal RezolveCallExpressionInfo ExtractRezolveCall(Expression e)
        {
            var methodExpr = e as MethodCallExpression;

            if (methodExpr == null || !methodExpr.Method.IsGenericMethod)
                return null;

            var match = RezolveMethods.SingleOrDefault(m => m.Equals(methodExpr.Method.GetGenericMethodDefinition()));

            if (match == null)
                return null;

            //by the number of the parameters we know if a string is being passed
            var nameParameter = methodExpr.Method.GetParameters().FirstOrDefault(pi => pi.ParameterType == typeof(string));

            //note below - firing the Visit method again for the parameter, which allows us to use Resolve operations
            //for the parameters.
            return nameParameter != null
                ? new RezolveCallExpressionInfo(methodExpr.Method.GetGenericArguments()[0], GetRezolveTarget(methodExpr.Arguments[0]))
                : new RezolveCallExpressionInfo(methodExpr.Method.GetGenericArguments()[0], null);
        }

        public IRezolveTarget GetRezolveTarget(Expression expression)
        {
            var result = Visit(expression) as RezolveTargetExpression;
            if (result != null)
                return result.Target;
            return null;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return new RezolveTargetExpression(new ObjectTarget(node.Value, node.Type));
        }

        protected override Expression VisitNew(NewExpression node)
        {
            return new RezolveTargetExpression(ConstructorTarget.For(node.Type, node, this));
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var constructorTarget = ConstructorTarget.For(node.Type, node.NewExpression, this);
            return new RezolveTargetExpression(new ExpressionTarget(c =>
            {
                var ctorTargetExpr = constructorTarget.CreateExpression(new CompileContext(c, node.Type));

                //the goal here, then, is to find the new expression for this type and replace it 
                //with a memberinit equivalent to the one we visited.  Although the constructor target produces 
                //a NewExpression, it isn't going to be the root expression, because of the scoping boilerplate 
                //that is put around nearly all expressions produced by RezolveTargetBase implementations. 
                var rewriter = new NewExpressionMemberInitRewriter(node.Type, node.Bindings.Select(mb => VisitMemberBinding(mb)));
                return rewriter.Visit(ctorTargetExpr);
            }, node.Type, this));
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Type == typeof(RezolveContextExpressionHelper))
                return new RezolveContextPlaceholderExpression(node);
            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            //we can't do anything special with lambdas - we just work over the body.  This enables
            //us to feed lambdas from code (i.e. compiler-generated expression trees) just as if we
            //were passing hand-built expressions.
            return base.Visit(node.Body);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //TODO: no string parameter here -needs to be reinstated.
            var rezolveCall = ExtractRezolveCall(node);
            if (rezolveCall != null)
                return new RezolveTargetExpression(new RezolvedTarget(rezolveCall));
            return base.VisitMethodCall(node);
        }

        public override Expression Visit(Expression node)
        {
            var result = base.Visit(node);
            if (result != null && !(result is RezolveTargetExpression))
                return new RezolveTargetExpression(new ExpressionTarget(result));
            return result;
        }
    }
}