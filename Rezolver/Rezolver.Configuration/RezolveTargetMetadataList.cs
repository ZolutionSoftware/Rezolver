using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class RezolveTargetMetadataList : RezolveTargetMetadataBase, IRezolveTargetMetadataList
	{
		private readonly List<IRezolveTargetMetadata> _targets;

		public RezolveTargetMetadataList()
			: this(null)
		{

		}

		public RezolveTargetMetadataList(IEnumerable<IRezolveTargetMetadata> range)
			: base(RezolveTargetMetadataType.MetadataList)
		{
			_targets = new List<IRezolveTargetMetadata>(range ?? Enumerable.Empty<IRezolveTargetMetadata>());
		}

		public IList<IRezolveTargetMetadata> Targets
		{
			get { return _targets; }
		}
	}
}
