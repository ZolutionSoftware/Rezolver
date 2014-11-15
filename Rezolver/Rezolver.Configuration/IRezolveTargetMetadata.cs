using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Represents an instruction to create an IRezolveTarget from parsed configuration.
	/// 
	/// The specific type of rezolve target to be created, or indeed how it is to be created, is dependant
	/// both on the implementation of the metadata itself as well as the adapter that is used to transform the 
	/// configuration into an IRezolverBuilder instance.
	/// </summary>
	public interface IRezolveTargetMetadata
	{
		/// <summary>
		/// The type of rezolve target that is expected to be produced from this metadata
		/// </summary>
		RezolveTargetMetadataType Type { get; }
	}
}
