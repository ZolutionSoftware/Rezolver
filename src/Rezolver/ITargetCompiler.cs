using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that produces <see cref="ICompiledTarget"/> instances from <see cref="ITarget"/> instances,
  /// and that can build Lambda expressions for the same targets
	/// </summary>
	public interface ITargetCompiler
	{
		/// <summary>
		/// Creates and builds a compiled target for the passed rezolve target which can then be used to
		/// create/obtain the object(s) it produces.
		/// </summary>
		/// <param name="target">The target to be compiled.</param>
		/// <param name="context">The current compilation context.</param>
		/// <returns>A compiled target that produces the object represented by <paramref name="target" />.</returns>
		ICompiledTarget CompileTarget(ITarget target, CompileContext context);
	}
}