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
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);

		/// <summary>
		/// Called to create the expression that will produce the object that is resolved by this target.  The expression
		/// might be expected to handle a dynamic rezolver being passed to it at run time to enable dynamic per-target overriding
		/// from other rezolvers.
		/// </summary>
		/// <param name="rezolver">The rezolver that defines the Builder in which this expression
		///   is being built.  Note that this is a 'compile-time' Builder and should be used during expression-building
		///   time to resolve any other targets that might be required for the statically compiled expression.</param>
		/// <param name="targetType">The type of object that the compiled expression is expected to produce.</param>
		/// <param name="dynamicRezolverExpression">Optional. If this is non-null, then the returned expression should cater for the 
		/// fact that a dynamic rezolver could be passed to any function built from this expression at run time.</param>
		/// <param name="currentTargets">Optional. A stack of targets that are currently being compiled - used to help detect
		/// circular dependencies between targets.</param>
		/// <returns></returns>
		Expression CreateExpression(IRezolver rezolver, Type targetType = null, ParameterExpression dynamicRezolverExpression = null, Stack<IRezolveTarget> currentTargets = null);
		Type DeclaredType { get; }
	}
}
