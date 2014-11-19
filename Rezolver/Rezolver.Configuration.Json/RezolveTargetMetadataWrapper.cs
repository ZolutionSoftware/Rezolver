using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	public class RezolveTargetMetadataWrapper : RezolveTargetMetadataBase, IRezolveTargetMetadataExtension
	{
		public const string ExtensionTypeName = "#JSONWRAPPER#";
		public string ExtensionType
		{
			get { return ExtensionTypeName; }
		}

		private IRezolveTargetMetadata _wrapped;

		public RezolveTargetMetadataWrapper(IRezolveTargetMetadata wrapped)
			: base(RezolveTargetMetadataType.Extension)
		{
			// TODO: Complete member initialization
			this._wrapped = wrapped;
		}
	}
}
