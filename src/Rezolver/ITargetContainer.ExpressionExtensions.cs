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
    public static partial class ExpressionTargetContainerExtensions
    {
		/// <summary>
		/// Registers the expression.
		/// </summary>
		/// <param name="targetContainer">The target container.</param>
		/// <param name="expression">The expression.</param>
		/// <param name="type">The type.</param>
		public static void RegisterExpression(this ITargetContainer targetContainer, Expression expression, Type type)
		{
			targetContainer.Register(new ExpressionTarget(expression), type);
		}
	}
}
