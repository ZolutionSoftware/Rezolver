using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rezolver.Diagnostics
{
	public class TrackedLifetimeScopeResolver : DefaultLifetimeScopeRezolver
	{
		private readonly int _id = TrackingUtils.NextRezolverID();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		private bool _disposed;
	
		protected internal ICallTracker Logger { get; private set; }

		public TrackedLifetimeScopeResolver(ICallTracker logger, 
			IRezolverBuilder builder = null,
			IRezolveTargetCompiler compiler = null, 
			ILifetimeScopeRezolver parentScope = null, 
			bool registerToBuilder = true)
			: base(builder: builder ?? new TrackedRezolverBuilder(logger), compiler: compiler, registerToBuilder:registerToBuilder)
		{
			logger.MustNotBeNull(nameof(logger));
			Logger = logger;
		}

		public override bool CanResolve(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.CanResolve(context), context);
		}
		
		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
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

		public object Resolve(RezolveContext context)
		{
			return Logger.TrackCall(this, () => base.Resolve(context), new { context = context });
		}

		public bool TryResolve(RezolveContext context, out object result)
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
	}
}
