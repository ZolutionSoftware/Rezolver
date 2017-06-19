// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver.Logging
{
	/// <summary>
	/// Logging version of <see cref="Container"/>
	/// 
	/// All method calls are logged (start/end/result/exception)
	/// </summary>
	public class TrackedContainer : Container
	{
		private readonly int _id = TrackingUtils.NextID<TrackedContainer>();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Tracker { get; private set; }


		public TrackedContainer(ICallTracker logger, ITargetContainer targets = null, IContainerConfig compilerConfig = null) :
		  base(targets: targets)
		{
			Tracker = logger;
            (compilerConfig ?? DefaultConfig).Configure(this, Targets);
		}

		public override bool CanResolve(IResolveContext context)
		{
			return Tracker.TrackCall(this, () => base.CanResolve(context), context);
		}

		public override IContainerScope CreateScope()
		{
			return Tracker.TrackCall(this, () => new TrackedContainerScope(Tracker, base.CreateScope()));
		}

		protected override ICompiledTarget GetCompiledTargetVirtual(IResolveContext context)
		{
			return Tracker.TrackCall(this, () => base.GetCompiledTargetVirtual(context), new { context = context });
		}

		protected override object GetService(Type serviceType)
		{
			return Tracker.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
		}

		public override object Resolve(IResolveContext context)
		{
			return Tracker.TrackCall(this, () => base.Resolve(context), new { context = context });
		}

		public override bool TryResolve(IResolveContext context, out object result)
		{
			object tempResult = null;
			var @return = Tracker.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}

		protected override ICompiledTarget GetFallbackCompiledTarget(IResolveContext context)
		{
			return Tracker.TrackCall(this, () => base.GetFallbackCompiledTarget(context), new { context = context });
		}
	}
}
