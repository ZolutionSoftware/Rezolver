using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that compiles delegates from IRezolveTarget instances.
	/// </summary>
	public interface IRezolveTargetCompiler
	{
		///<summary>
		/// Creates and builds a compiled target for the passed rezolve target which can then be used to 
		/// fetch the object(s) it produces.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="containerScope">The scope within which this target is to be compiled - this will be used
		/// to look up any other targets to be used as dependencies for the statically compiled delegate.</param>
		/// <param name="dynamicContainerExpression">When this is the expression to be used to represent the
		/// dynamic container parameter used in the dynamic rezolve calls.  If null, then a default
		/// should be used if required - the <see cref="ExpressionHelper.DynamicContainerParam"/></param>
		/// <param name="targetStack">Optional - if this compilation is taking place as part of a wider compilation
		/// then this is used to pass the stack of targets that are already compiling.  Generally you will pass this
		/// as null.</param>
		/// <returns>A compiled target that produces the object represented by <paramref name="target"/> via multiple
		/// different method.</returns>
		ICompiledRezolveTarget CompileTarget(IRezolveTarget target, IRezolverContainer containerScope,
			ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> targetStack = null);
	}
}