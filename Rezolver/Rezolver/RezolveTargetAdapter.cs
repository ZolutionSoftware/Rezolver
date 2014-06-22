using System;
using System.Linq;
using System.Linq.Expressions;
using Rezolver.Resources;

namespace Rezolver
{
	/// <summary>
	/// Default implementation of the IRezolveTargetAdapter interface
	/// 
	/// </summary>
	public class RezolveTargetAdapter : ExpressionVisitor, IRezolveTargetAdapter
	{

		internal static RezolveCallExpressionInfo ExtractRezolveCall(Expression e)
		{
			var methodExpr = e as MethodCallExpression;

			if (methodExpr == null || !methodExpr.Method.IsGenericMethod)
				return null;

			var match = RezolverScopeExtensions.RezolveMethods.SingleOrDefault(m => m.Equals(methodExpr.Method.GetGenericMethodDefinition()));

			if (match == null)
				return null;

			//by the number of the parameters we know if a string is being passed
			var nameParameter = methodExpr.Method.GetParameters().FirstOrDefault(pi => pi.ParameterType == typeof(string));

			return nameParameter != null
				? new RezolveCallExpressionInfo(methodExpr.Method.GetGenericArguments()[0], methodExpr.Arguments[1])
				: new RezolveCallExpressionInfo(methodExpr.Method.GetGenericArguments()[0], null);
		}

		internal class RezolveCallExpressionInfo
		{
			public Type Type { get; private set; }
			public Expression Name { get; private set; }

			internal RezolveCallExpressionInfo(Type type, Expression name)
			{
				Type = type;
				Name = name;
			}
		}

		public IRezolveTarget ConvertToTarget(Expression expression)
		{
			var visitor = new TargetFactoryVisitor();
			return visitor.BuildTarget(expression);
		}

		public class TargetFactoryVisitor : ExpressionVisitor
		{
			public IRezolveTarget BuildTarget(Expression expression)
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
				var ctor = node.Constructor;// ?? node.Type.GetConstructor(Type.EmptyTypes);
				if(ctor == null)
					throw new ArgumentException(Exceptions.NoConstructorSetOnNewExpression, "node");

				var parameters = ctor.GetParameters();
				return new RezolveTargetExpression(new ConstructorTarget(node.Type, node.Constructor,
					node.Arguments.Select((pExp, i) => new ParameterBinding(parameters[i], BuildTarget(node))).ToArray()));
			}

			protected override Expression VisitLambda<T>(Expression<T> node)
			{
				return base.Visit(node.Body);
			}

			protected override Expression VisitMethodCall(MethodCallExpression node)
			{
				//TODO: no string parameter here -needs to be reinstated.
				var rezolveCall = ExtractRezolveCall(node);
				if(rezolveCall != null)
					return new RezolveTargetExpression(new RezolvedTarget(rezolveCall));
				return base.VisitMethodCall(node);
			}

			public override Expression Visit(Expression node)
			{
				var result = base.Visit(node);
				return result;
			}
		}
	}
}