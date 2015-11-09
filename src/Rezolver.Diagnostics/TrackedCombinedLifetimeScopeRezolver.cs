using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Diagnostics
{
	public class TrackedCombinedLifetimeScopeRezolver : CombinedLifetimeScopeRezolver
	{
		private readonly int _id = TrackingUtils.NextRezolverID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Logger { get; private set; }

		internal TrackedCombinedLifetimeScopeRezolver(TrackedCombinedLifetimeScopeRezolver parent,
			IRezolverBuilder builder = null,
			IRezolveTargetCompiler compiler = null) 
			: this(parent.Logger, parent, builder: builder, compiler: compiler)
		{

		}

		internal TrackedCombinedLifetimeScopeRezolver(TrackedLifetimeScopeResolver parent,
			IRezolverBuilder builder = null,
			IRezolveTargetCompiler compiler = null)
			: this(parent.Logger, parent, builder: builder, compiler: compiler)
		{

		}

		public TrackedCombinedLifetimeScopeRezolver(ICallTracker logger, 
			ILifetimeScopeRezolver parentScope, 
			IRezolver inner = null, 
			IRezolverBuilder builder = null, 
			IRezolveTargetCompiler compiler = null)
			: base(parentScope, inner, builder ?? new TrackedRezolverBuilder(logger), compiler)
		{
			Logger = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.CanResolve(context), context);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//TODO: change this to a LoggingCombinedLifetimeScopeRezolver
			return Logger.TrackCall(this, () => new TrackedCombinedLifetimeScopeRezolver(this));
		}

		public override ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.FetchCompiled(context), new { context = context });
		}

		protected override object GetService(Type serviceType)
		{
			return Logger.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
		}

		public override object Resolve(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.Resolve(context), new { context = context });
		}

		public override bool TryResolve(RezolveContext context, out object result)
		{
			object tempResult = null;
			var @return = Logger.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}

		public override void AddToScope(object obj, RezolveContext context = null)
		{
			Logger.TrackCall(this, () => base.AddToScope(obj, context), new { obj = obj, context = context });
		}

		public override IEnumerable<object> GetFromScope(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetFromScope(context), new { context = context });
		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
		}

		protected override ICompiledRezolveTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
		}
	}
}
