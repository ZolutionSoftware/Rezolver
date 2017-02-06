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
	public class TrackedScopeContainer : ScopedContainer
	{
		private readonly int _id = TrackingUtils.NextID<TrackedScopeContainer>();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Logger { get; private set; }

		public TrackedScopeContainer(ICallTracker logger,
		  ITargetContainer builder = null,
		  ICompilerConfigurationProvider compilerConfig = null,
		  IScopedContainer parentScope = null)
		  : base(builder: builder ?? new TrackedTargetContainer(logger), compilerConfig: compilerConfig)
		{
			logger.MustNotBeNull(nameof(logger));
			Logger = logger;
		}
		protected override void Dispose(bool disposing)
		{
			Logger.TrackCall(this, () => base.Dispose(disposing));
		}
		public override bool CanResolve(ResolveContext context)
		{
			return Logger.TrackCall(this, () => base.CanResolve(context), context);
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
	}
}
