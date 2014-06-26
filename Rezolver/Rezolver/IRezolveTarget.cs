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
	/// </summary>
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);

		/// <summary>
		/// Called to create the expression that will produce the object that is resolved by this target.
		/// </summary>
		/// <param name="containerScope">The rezolver container that defines the scope in which this expression
		///   is being built.  Note that this is a 'compile-time' scope and should be used during expression-building
		///   time to resolve any other targets that might be required </param>
		/// <param name="targetType"></param>
		/// <param name="currentTargets"></param>
		/// <returns></returns>
		Expression CreateExpression(IRezolverContainer containerScope, Type targetType = null, Stack<IRezolveTarget> currentTargets = null);
		Type DeclaredType { get; }
	}
}
