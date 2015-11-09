using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Diagnostics
{
	/// <summary>
	/// A rezolver that logs all calls through the IRezolver interface to aid debugging.
	/// </summary>
	public class TrackedDefaultRezolver : DefaultRezolver
	{
		private readonly int _id = TrackingUtils.NextRezolverID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Tracker { get; private set; }


		public TrackedDefaultRezolver(ICallTracker logger, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null, bool registerToBuilder = true) :
			base(builder: builder, compiler: compiler, registerToBuilder: registerToBuilder)
		{
			Tracker = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Tracker.TrackCall(this, () => base.CanResolve(context), context);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return Tracker.TrackCall(this, () => new TrackedCombinedLifetimeScopeRezolver(Tracker, null, this));
		}

		public override ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			return Tracker.TrackCall(this, () => base.FetchCompiled(context), new { context = context });
		}

		protected override object GetService(Type serviceType)
		{
			return Tracker.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
		}

		public override object Resolve(RezolveContext context)
		{
			return Tracker.TrackCall(this, () => base.Resolve(context), new { context = context });
		}

		public override bool TryResolve(RezolveContext context, out object result)
		{
			object tempResult = null;
			var @return = Tracker.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			return Tracker.TrackCall(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
		}

		protected override ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return Tracker.TrackCall(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
		}
	}
}
