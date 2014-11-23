using System;
namespace Rezolver.Configuration
{
	/// <summary>
	/// Interface for metadata for constructing an ObjectTarget IRezolveTarget.
	/// </summary>
	public interface IObjectTargetMetadata : IRezolveTargetMetadata
	{
		/// <summary>
		/// Called to get the object that will be registered in the IRezolverBuilder to be returned when a 
		/// caller requests one of it's registered types. The method can construct an object anew everytime it is
		/// called, or it can always return the same instance; this behaviour is implementation-dependant.
		/// </summary>
		/// <param name="type">The type of object that is desired.  The implementation determines whether this
		/// parameter is required.  If it is, and you pass null, then an ArgumentNullException will be thrown.
		/// If you pass an argument, and the object cannot be constructed for the given type, then an ArgumentException
		/// will be thrown.</param>
		/// <returns>An object.  Note - if the operation returns null this is not an error.</returns>
		/// <exception cref="System.ArgumentNullException">If <paramref name="type"/> is required by the implementation in 
		/// order to build the object.</exception>
		/// <exception cref="System.ArgumentException">If an object cannot be built for the given type.</exception>
		object GetObject(Type type);
	}
}
