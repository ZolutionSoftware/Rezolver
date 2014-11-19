using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class RezolveTargetMetadataBase : IRezolveTargetMetadata
	{
		public RezolveTargetMetadataType Type
		{
			get;
			private set;
		}

		protected RezolveTargetMetadataBase(RezolveTargetMetadataType type)
		{
			if (type == RezolveTargetMetadataType.Extension && !(this is IRezolveTargetMetadataExtension))
				throw new ArgumentException("If type is extension this instance must implement IRezolveTargetMetadataExtension");
			Type = type;
		}
	}
}
