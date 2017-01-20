// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver.Logging
{
	public class TrackedOverridingScopedContainer : OverridingScopedContainer
	{
		private readonly int _id = TrackingUtils.NextID<TrackedOverridingScopedContainer>();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Logger { get; private set; }

		internal TrackedOverridingScopedContainer(TrackedOverridingScopedContainer parent,
		  ITargetContainer builder = null,
		  ITargetCompiler compiler = null)
		  : this(parent.Logger, parent, builder: builder, compiler: compiler)
		{

		}

		internal TrackedOverridingScopedContainer(TrackedScopeContainer parent,
		  ITargetContainer builder = null,
		  ITargetCompiler compiler = null)
		  : this(parent.Logger, parent, builder: builder, compiler: compiler)
		{

		}

		public TrackedOverridingScopedContainer(ICallTracker logger,
		  IScopedContainer parentScope,
		  IContainer inner = null,
		  ITargetContainer builder = null,
		  ITargetCompiler compiler = null)
		  : base(parentScope, inner, builder ?? new TrackedTargetContainer(logger), compiler)
		{
			Logger = logger;
		}

		protected override void Dispose(bool disposing)
		{
			Logger.TrackCall(this, (callId) =>
			{
				var allTrackedObjects = this.TrackedObjects;
				Logger.Message(callId, MessageType.Information, format: $"{ this } being disposed - { allTrackedObjects.Count() } child objects are being disposed.");
				Logger.Message(callId, MessageType.Debug, "Objects being disposed:");
				foreach(var o in allTrackedObjects)
				{
					Logger.Message(callId, MessageType.Debug, format: $"{ o }");
				}

				base.Dispose(disposing);
			});
		}

		public override bool CanResolve(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.CanResolve(context), new { context = context });
		}

		public override IScopedContainer CreateLifetimeScope()
		{
			return Logger.TrackCall(this, () => new TrackedOverridingScopedContainer(this));
		}

		public override ICompiledTarget FetchCompiled(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.FetchCompiled(context), new { context = context });
		}

		protected override object GetService(Type serviceType)
		{
			return Logger.TrackCall(this, () => base.GetService(serviceType), new { serviceType = serviceType });
		}

		public override object Resolve(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.Resolve(context), new { context = context });
		}

		public override bool TryResolve(ResolveContext context, out object result)
		{
			object tempResult = null;
			var @return = Logger.TrackCall(this, () => base.TryResolve(context, out tempResult), new { context = context });
			result = tempResult;
			return @return;
		}

		public override void AddToScope(object obj, ResolveContext context = null)
		{
			Logger.TrackCall(this, () => base.AddToScope(obj, context), new { obj = obj, context = context });
		}

		public override IEnumerable<object> GetFromScope(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetFromScope(context), new { context = context });
		}

		protected override ICompiledTarget GetCompiledRezolveTarget(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetCompiledRezolveTarget(context), new { context = context });
		}

		protected override ICompiledTarget GetFallbackCompiledRezolveTarget(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.GetFallbackCompiledRezolveTarget(context), new { context = context });
		}
	}
}
