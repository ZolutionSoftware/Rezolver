// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Concurrent;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Not sure if this is sticking around anyway.  Think the scoping stuff needs a bit of a rethink.
	/// </summary>
	public class OverridingScopedContainer : OverridingContainer, IScopedContainer
	{
		private ConcurrentDictionary<ResolveContext, ConcurrentBag<object>> _objects;
		protected IEnumerable<object> TrackedObjects
		{
			get
			{
				return _objects.Values.SelectMany(cbag => cbag.AsEnumerable()).ToArray();
			}
		}

		public IScopedContainer ParentScope
		{
			get
			{
				return _parentScope;
			}
		}

		private bool _disposed;
		private readonly IScopedContainer _parentScope;

		public event EventHandler Disposed;

		/// <summary>
		/// Constructs a new instance of the CombinedLifetimeScopeRezolver class.
		/// </summary>
		/// <param name="parentScope">Can be null, but if it is, then <paramref name="inner"/> must be supplied</param>
		/// <param name="inner">Can be null, but if it is, then <paramref name="parentScope"/> must be supplied</param>
		/// <param name="builder"></param>
		/// <param name="compiler"></param>
		public OverridingScopedContainer(IScopedContainer parentScope, IContainer inner = null, ITargetContainer builder = null, ICompilerConfigurationProvider compilerConfig = null)
			: base(inner ?? parentScope, targets: builder, compilerConfig: compilerConfig)
		{
			_parentScope = parentScope;
			_objects = new ConcurrentDictionary<ResolveContext, ConcurrentBag<object>>();
			_disposed = false;

			if (_parentScope != null)
				_parentScope.Disposed += ParentScope_Disposed;
		}

		private void ParentScope_Disposed(object sender, EventArgs e)
		{
			//when a parent scope is disposed, this scope must also be disposed.
			Dispose();
		}

		protected void OnDisposed()
		{
			try
			{
				Disposed?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception) { } //don't want an exception here to break anything
		}

		private void TrackObject(object obj, ResolveContext context)
		{
			if (obj == null)
				return;
			ConcurrentBag<object> instances = _objects.GetOrAdd(
				new ResolveContext(null, context.RequestedType),
				c => new ConcurrentBag<object>());

			//bit slow this, but hopefully there won't be loads of them...
			//note that there'll be a memory overhead with this, certainly in portable,
			//as under the hood the internal implementation realises the enumerable as an array
			//before returning its enumerator.
			if (!instances.Any(o => object.ReferenceEquals(o, obj)))
				instances.Add(obj);
		}



		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				object obj = null;
				IDisposable disposableObj = null;
				foreach (var kvp in _objects)
				{
					while (kvp.Value.TryTake(out obj))
					{
						disposableObj = obj as IDisposable;
						if (disposableObj != null)
							disposableObj.Dispose();
					}
				}
				_objects.Clear();
				OnDisposed();
			}
			_disposed = true;
		}

		public virtual void AddToScope(object obj, ResolveContext context = null)
		{
			obj.MustNotBeNull("obj");
			TrackObject(obj, context ?? new ResolveContext(null, obj.GetType()));
		}

		public virtual IEnumerable<object> GetFromScope(ResolveContext context)
		{
			context.MustNotBeNull("context");
			ConcurrentBag<object> instances = null;
			if (_objects.TryGetValue(context, out instances))
			{
				//important to return a read-only collection here to avoid modification
				return new ReadOnlyCollection<object>(instances.ToArray());
			}
			else
				return Enumerable.Empty<object>();
		}
	}


}