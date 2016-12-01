using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver
{
	/// <summary>
	/// Extensions for to simplify registering expressions in an <see cref="ITargetContainer"/>.
	/// </summary>
    public static class TargetAdapterRegisterExpressionExtensions
    {
		/// <summary>
		/// Registers the expression.
		/// </summary>
		/// <param name="targetContainer">The target container.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="type">The type.</param>
		/// <param name="adapter">The adapter.</param>
		public static void RegisterExpression(this ITargetContainer targetContainer, Expression expression, Type type, ITargetAdapter adapter = null)
		{
			var target = (adapter ?? TargetAdapter.Default).CreateTarget(expression);
			targetContainer.Register(target, type);
		}
	}
}
