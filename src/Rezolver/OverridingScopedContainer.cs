using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Concurrent;

namespace Rezolver
{
	public class OverridingScopedContainer : OverridingContainer, IScopedContainer
	{
		private ConcurrentDictionary<RezolveContext, ConcurrentBag<object>> _objects;

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
		public OverridingScopedContainer(IScopedContainer parentScope, IContainer inner = null, ITargetContainer builder = null, ITargetCompiler compiler = null)
				: base(inner ?? parentScope, builder: builder, compiler: compiler)
		{
			_parentScope = parentScope;
			_objects = new ConcurrentDictionary<RezolveContext, ConcurrentBag<object>>();
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

		private void TrackObject(object obj, RezolveContext context)
		{
			if (obj == null)
				return;
			ConcurrentBag<object> instances = _objects.GetOrAdd(
					new RezolveContext(null, context.RequestedType),
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

		public virtual void AddToScope(object obj, RezolveContext context = null)
		{
			obj.MustNotBeNull("obj");
			TrackObject(obj, context ?? new RezolveContext(null, obj.GetType()));
		}

		public virtual IEnumerable<object> GetFromScope(RezolveContext context)
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