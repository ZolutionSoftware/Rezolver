using System;
namespace Rezolver
{
	/// <summary>
	/// Provides an abstraction for creating objects in response to a resolve operation.
	/// 
	/// As the name suggests, it typically represents a compiled <see cref="ITarget"/>, 
	/// which, in the standard <see cref="IContainer"/> implementation (<see cref="Container"/>)
	/// is the final stage before its ready to be used to start producing objects.
	/// 
	/// An <see cref="ITargetCompiler"/> is responsible for creating these from one or more <see cref="ITarget"/>s.
	/// </summary>
	public interface ICompiledTarget
	{
		/// <summary>
		/// Called to get/create an object, potentially using the passed <paramref name="context"/> to aid resolve additional dependencies.
		/// </summary>
		/// <param name="context">The current rezolve context.</param>
		/// <returns>The object that is constructed.  The return value can legitimately be null.</returns>
		object GetObject(RezolveContext context);
	}
}
