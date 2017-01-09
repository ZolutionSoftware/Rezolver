using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised for building expressions for the <see cref="ListTarget"/> target.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.ListTarget}" />
	public class ListTargetBuilder : ExpressionBuilderBase<ListTarget>
	{
		/// <summary>
		/// Builds an expression which either represents creating an array or a list of objects using an 
		/// enumerable of targets from the <paramref name="target"/>'s <see cref="ListTarget.Items"/>.
		/// 
		/// The target's <see cref="ListTarget.AsArray"/> flag is used to determine which expression to build.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		protected override Expression Build(ListTarget target, CompileContext context, IExpressionCompiler compiler)
		{
			var arrayExpr = Expression.NewArrayInit(target.ElementType,
					target.Items.Select(t => compiler.Build(t, context.New(target.ElementType))));

			if (target.AsArray)
				return arrayExpr;
			else
				return Expression.New(target.ListConstructor, arrayExpr);
		}
	}
}
