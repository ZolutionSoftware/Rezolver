using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// As the name suggests, the underlying target of a Rezolver call.  The output of a 
	/// target is an expression.  This allows a target that depends on another
	/// target to chain expressions together, creating specialised expression trees (and
	/// therefore specialised delegates).
	/// 
	/// The expression produced from this interface is later compiled, by an IRezolveTargetCompiler,
	/// into an ICompiledRezolveTarget - whose job it is specifically to produce object instances.
	/// </summary>
	public interface ITarget
	{
		/// <summary>
		/// If true, it is an instruction to any consumer to consider falling back to a better target
		/// configured in a more authoritative builder.  In general - almost all targets return
		/// false for this.
		/// </summary>
		bool UseFallback { get; }
		/// <summary>
		/// Gets the static type produced by this target, when executing the expression returned from a call to 
		/// <see cref="CreateExpression"/> without providing your own explicit type to be returned.
		/// </summary>
		/// <value>The type of the declared.</value>
		Type DeclaredType { get; }
		/// <summary>
		/// Returns a boolean indicating whether the target is able to produce an instance of, or an instance
		/// that is compatible with, the passed <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if <paramref name="type"/> is supported, <c>false</c> otherwise.</returns>
		bool SupportsType(Type type);
		/// <summary>
		/// Called to create the expression that will produce the object that is resolved by this target.  The expression
		/// might be expected to handle a dynamic rezolver being passed to it at run time to enable dynamic per-target overriding
		/// from other rezolvers.
		/// </summary>
		/// <param name="context">The current compile context</param>
		/// <returns></returns>
		Expression CreateExpression(CompileContext context);

	}
}
