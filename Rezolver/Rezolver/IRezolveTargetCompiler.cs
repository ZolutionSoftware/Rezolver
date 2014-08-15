using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that produces ICompiledRezolveTarget instances from IRezolveTarget instances.
	/// </summary>
	public interface IRezolveTargetCompiler
	{
		///<summary>
		/// Creates and builds a compiled target for the passed rezolve target which can then be used to 
		/// create/obtain the object(s) it produces.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="rezolver">The rezolver that defines the Builder in which this expression
		///   is being built.  Note that this is a 'compile-time' Builder and should be used during expression-building
		///   time to resolve any other targets that might be required for the statically compiled expression.</param>
		/// <param name="dynamicRezolverExpression">Optional. Used to define the parameter expression to be used in 
		/// the code compiled into the resulting target that represents the dynamic rezolver that is passed to the
		/// <see cref="ICompiledRezolveTarget.GetObjectDynamic"/> method.</param>
		/// <param name="currentTargets">Optional. A stack of targets that are currently being compiled - used to help detect
		/// circular dependencies between targets.</param>
		/// <returns>A compiled target that produces the object represented by <paramref name="target"/>.</returns>
		[Obsolete("This will be removed in favour of the CompileContext version", true)]
		ICompiledRezolveTarget CompileTarget(IRezolveTarget target, 
			IRezolver rezolver,
			ParameterExpression dynamicRezolverExpression = null, 
			Stack<IRezolveTarget> targetStack = null);

		ICompiledRezolveTarget CompileTarget(IRezolveTarget target, CompileContext context);
	}
}