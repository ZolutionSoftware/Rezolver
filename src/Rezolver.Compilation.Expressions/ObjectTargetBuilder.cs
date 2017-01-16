using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// An <see cref="IExpressionBuilder"/> specialised to build expressions for the <see cref="ObjectTarget"/> target.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.ObjectTarget}" />
	public class ObjectTargetBuilder : ExpressionBuilderBase<ObjectTarget>
	{
		/// <summary>
		/// returns a ConstantExpression wrapped around the <see cref="ObjectTarget.Value"/> reference.
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		/// <exception cref="NotImplementedException"></exception>
		protected override Expression Build(ObjectTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
		{
			return Expression.Constant(target.Value, context.TargetType ?? target.DeclaredType);
		}
	}
}
