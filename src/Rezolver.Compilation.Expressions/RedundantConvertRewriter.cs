using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Removes unnecessary convert expressions from an expression.
	/// 
	/// An unnecessary conversion is one where the target type is equal to, or a base of, the source type.
	/// 
	/// Only boxing/unboxing conversions or upcasts are left intact.
	/// </summary>
	/// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
	public class RedundantConvertRewriter : ExpressionVisitor
    {
		/// <summary>
		/// Visits the children of the <see cref="T:System.Linq.Expressions.UnaryExpression" />.
		/// </summary>
		/// <param name="node">The expression to visit.</param>
		protected override Expression VisitUnary(UnaryExpression node)
		{
			if (node.NodeType == ExpressionType.Convert &&
			  node.Type == node.Operand.Type ||
			  (!TypeHelpers.IsValueType(node.Operand.Type) && TypeHelpers.IsAssignableFrom(node.Type, node.Operand.Type)))
				return node.Operand;

			return base.VisitUnary(node);
		}
	}
}
