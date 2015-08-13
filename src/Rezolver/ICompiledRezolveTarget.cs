using System;
namespace Rezolver
{
	/// <summary>
	/// Provides an abstraction for creating objects in response to a resolve operation.
	/// 
	/// As the name suggests, it typically represents a compiled <see cref="IRezolveTarget"/>, 
	/// which, in the standard <see cref="IRezolver"/> implementation (<see cref="DefaultRezolver"/>)
	/// is the final stage before its ready to be used to start producing objects.
	/// 
	/// An <see cref="IRezolveTargetCompiler"/> is responsible for creating these from the targets.
	/// </summary>
	public interface ICompiledRezolveTarget
	{
		/// <summary>
		/// Called to get/create an object, potentially using the passed <paramref name="context"/> to aid resolve additional dependencies.
		/// </summary>
		/// <param name="context">The current rezolve context.</param>
		/// <returns>The object that is constructed.  The return value can legitimately be null.</returns>
		object GetObject(RezolveContext context);
	}
}
