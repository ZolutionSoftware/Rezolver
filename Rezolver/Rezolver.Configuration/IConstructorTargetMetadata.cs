using System;
using System.Collections.Generic;
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
		/// <summary>
		/// Gets a dictionary of arguments that are to be supplied to the object's constructor
		/// </summary>
		/// <value>The arguments.</value>
		IDictionary<string, IRezolveTargetMetadata> Arguments { get; }
		/// <summary>
		/// Gets the types of the parameters for the specific constructor that is to be bound.  Not required, and is mostly
		/// used when a suitable constructor cannot be found purely by matching parameter names and types to the <see cref="Arguments"/>.
		/// 
		/// A common issue here being that some metadata types can build any type, therefore two constructors with identically named
		/// parameters that have different types could be matched by the same target metadata.
		/// 
		/// If null, then no signature is specified.
		/// </summary>
		/// <value>The signature types.</value>
		ITypeReference[] SignatureTypes { get; }
	}
}
