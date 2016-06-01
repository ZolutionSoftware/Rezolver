using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Diagnostics
{
	public class TrackedRezolverBuilder : RezolverBuilder
	{ 
		private readonly int _id = TrackingUtils.NextBuilderID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		public ICallTracker Logger { get; private set; }
		/// <summary>
		/// Creates a new instance that wraps around the passed IRezolverBuilder.
		/// </summary>
		/// <param name="logger">The logger that is to receive logging calls from this instance.</param>
		public TrackedRezolverBuilder(ICallTracker logger)
		{
			Logger = logger;
		}

		protected override IRezolveTargetEntry CreateEntry(Type type, params IRezolveTarget[] targets)
		{
			return Logger.TrackCall(this, () => base.CreateEntry(type, targets), new { type = type, targets = targets });
		}

		
		public override IRezolveTargetEntry Fetch(Type type)
		{
			return base.Fetch(type);
		}

		public override void Register(IRezolveTarget target, Type type = null)
		{
			base.Register(target, type);
		}
	}
}
