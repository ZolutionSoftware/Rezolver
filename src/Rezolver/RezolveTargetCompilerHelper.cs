using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	public static class RezolveTargetCompilerHelper
	{
		public class RedundantConvertRemover : ExpressionVisitor
		{
			public RedundantConvertRemover()
			{

			}

			protected override Expression VisitUnary(UnaryExpression node)
			{
				if (node.NodeType == ExpressionType.Convert &&
					node.Type == node.Operand.Type ||
					(!TypeHelpers.IsValueType(node.Operand.Type) && TypeHelpers.IsAssignableFrom(node.Type, node.Operand.Type)))
					return node.Operand;

				return base.VisitUnary(node);
			}
		}

		private static readonly RedundantConvertRemover _redundantConvertRemover = new RedundantConvertRemover();

		public static Expression Optimise(this Expression expression, IEnumerable<Func<Expression, Expression>> additionalOptimisations = null)
		{
			expression = _redundantConvertRemover.Visit(expression);
			foreach(var optimisation in (additionalOptimisations ?? Enumerable.Empty<Func<Expression, Expression>>()))
			{
				expression = optimisation(expression);
			}
			return expression;
		}
	}
}
