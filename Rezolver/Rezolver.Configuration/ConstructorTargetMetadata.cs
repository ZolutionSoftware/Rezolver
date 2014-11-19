using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ConstructorTargetMetadata : RezolveTargetMetadataBase
	{
		private ITypeReference[] targetType;

		public ConstructorTargetMetadata(ITypeReference[] targetTypes)
			: base(RezolveTargetMetadataType.Constructor)
		{
			// TODO: Complete member initialization
			this.targetType = targetTypes;
		}
	}
}
