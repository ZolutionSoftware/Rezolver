using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Records one or more IRezolveTargets that have been registered against a given type.
	/// </summary>
	public interface IRezolveTargetEntry : IRezolveTarget
	{
		IRezolveTarget DefaultTarget { get; }
		IEnumerable<IRezolveTargetEntry> ChildEntries { get; }
	}
}
