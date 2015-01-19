using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// Represents a list of IRezolveTargetMetadata instances - for when you want to register multiple targets against a single type.
	/// </summary>
	public interface IRezolveTargetMetadataList : IRezolveTargetMetadata
	{
		/// <summary>
		/// Gets the list of targets that will be used to construct the array.
		/// 
		/// Note - a list is used to allow for modification of the targets after initial creation.
		/// </summary>
		/// <value>The targets.</value>
		IList<IRezolveTargetMetadata> Targets { get; }
	}
}
