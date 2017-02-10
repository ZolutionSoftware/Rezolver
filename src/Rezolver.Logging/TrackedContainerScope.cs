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
	public class TrackedContainerScope : IContainerScope
	{
		private readonly int _id = TrackingUtils.NextID<TrackedContainerScope>();

		public override string ToString()
		{
			return $"(#{_id} {GetType().Name})";
		}

		protected internal ICallTracker Logger { get; }

		private readonly IContainerScope _inner;

		private IContainerScope _parent;
		public IContainerScope Parent
		{
			get
			{
				return _parent ?? _inner.Parent;
			}
		}

		public IContainer Container
		{
			get
			{
				return _inner.Container;
			}
		}

		public TrackedContainerScope(ICallTracker logger,
			IContainerScope inner, TrackedContainerScope parent = null)
		{
			Logger = logger;
			_inner = inner;
			//parent is overriden when creating a tracked scope from a tracked scope
			_parent = parent;
		}

		public IContainerScope CreateScope()
		{
			return Logger.TrackCall(this,() => new TrackedContainerScope(Logger, _inner.CreateScope(), this));
		}

		public void ChildScopeDisposed(IContainerScope child)
		{
			Logger.TrackCall(this, () => _inner.ChildScopeDisposed(child));
		}

		public object Resolve(ResolveContext context, Func<ResolveContext, object> factory, ScopeActivationBehaviour behaviour)
		{
			return Logger.TrackCall(this, () => _inner.Resolve(context, factory, behaviour));
		}

		public void Dispose()
		{
			Logger.TrackCall(this, () => _inner.Dispose());
		}

		public object GetService(Type serviceType)
		{
			return Logger.TrackCall(this, () => _inner.GetService(serviceType));
		}
	}
}
