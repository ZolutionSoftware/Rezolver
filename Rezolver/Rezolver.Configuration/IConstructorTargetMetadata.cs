using System;
namespace Rezolver.Configuration
{
	public interface IConstructorTargetMetadata : IRezolveTargetMetadata
	{
		/// <summary>
		/// One of these types will be selected to have its constructor bound.  The rule is that multiple types passed here
		/// must represent types that all appear in an inheritance chain or interface list, and there must be one unambiguous
		/// most-derived type which will be the one whose constructor will be executed when an object is later dished out from the
		/// IRezolveTarget instance that is built from this metadata.
		/// </summary>
		ITypeReference[] TypesToBuild { get; }
	}
}
