using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public interface IRezolveTargetMetadataExtension : IRezolveTargetMetadata
	{
		string ExtensionType { get; }
	}
}
