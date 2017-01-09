using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
	/// <summary>
	/// Specialised builder for <see cref="DecoratorTarget"/> targets.
	/// </summary>
	/// <seealso cref="Rezolver.Compilation.Expressions.ExpressionBuilderBase{Rezolver.DecoratorTarget}" />
	public class DecoratorTargetBuilder : ExpressionBuilderBase<DecoratorTarget>
	{
		/// <summary>
		/// Creates a new compilation context, registers the target's <see cref="DecoratorTarget.DecoratedTarget"/>
		/// into it as the correct target for the <see cref="DecoratorTarget.DecoratedType"/>, and then builds the 
		/// expression for the <see cref="DecoratorTarget.Target"/> (which is typically a constructor target).
		/// </summary>
		/// <param name="target">The target whose expression is to be built.</param>
		/// <param name="context">The compilation context.</param>
		/// <param name="compiler">The expression compiler to be used to build any other expressions for targets
		/// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
		/// parameter is optional, this will always be provided</param>
		protected override Expression Build(DecoratorTarget target, CompileContext context, IExpressionCompiler compiler)
		{
			//need a new context for this, into which we can override the registration of the decorated type
			//to be the decorated target so that the decorator target will resolve that.
			//there's a potential hole here in that if another container resolves this same decorator after it
			//has been compiled, it might end up decorating itself - might need a test scenario for that.

			var newContext = new CompileContext(context, inheritSharedExpressions: true);
			//add the decorated target into the compile context under the type which the enclosing decorator
			//was registered against.  If the inner target is bound to a type which correctly implements the decorator
			//pattern over the common decorated type, then the decorated instance should be resolved when constructor
			//arguments are resolved.
			newContext.Register(target.DecoratedTarget, target.DecoratedType);
			return compiler.Build(target.Target, newContext);
		}
	}
}
