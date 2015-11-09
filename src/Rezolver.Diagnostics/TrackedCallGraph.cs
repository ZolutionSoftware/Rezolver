using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Diagnostics
{
	/// <summary>
	/// Represents a series of tracked calls, potentially to different objects, that have been 
	/// tracked via an <see cref="ICallTracker"/>
	/// </summary>
	public class TrackedCallGraph
	{
		public IEnumerable<TrackedCall> Calls { get; private set; }

		public TrackedCallGraph(IEnumerable<TrackedCall> calls)
		{
			calls.MustNotBeNull(nameof(calls));
			Calls = calls;
		}
	}
}
