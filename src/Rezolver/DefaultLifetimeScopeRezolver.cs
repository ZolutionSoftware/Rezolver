using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Extends the DefaultRezolver to implement lifetime scoping.
	/// 
	/// If you want your root rezolver to act as a lifetime scope, then you should use this
	/// class instead of using <see cref="DefaultRezolver"/>
	/// </summary>
	/// <remarks>The implementation of this class is very similar to the <see cref="CombinedLifetimeScopeRezolver"/>,
	/// The main difference being that that class can accept additional registrations independent of those
	/// in the rezolver that it's created from, whereas with this class, it *is* the rezolver.
	/// 
	/// This type is therefore suited only for standalone Rezolvers for which you want lifetime scoping
	/// and disposable handling; whereas the <see cref="CombinedLifetimeScopeRezolver"/> is primarily
	/// suited for use as a child lifetime scope for another rezolver.</remarks>
	public class DefaultLifetimeScopeRezolver : DefaultRezolver, ILifetimeScopeRezolver
	{
		private ConcurrentBag<ILifetimeScopeRezolver> _children;
		private ConcurrentDictionary<RezolveContext, ConcurrentBag<object>> _objects;

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				return null;
			}
		}

		private bool _disposed;
		public DefaultLifetimeScopeRezolver(IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null, bool registerToBuilder = true)
				: base(builder, compiler, registerToBuilder)
		{
			_children = new ConcurrentBag<ILifetimeScopeRezolver>();
			_objects = new ConcurrentDictionary<RezolveContext, ConcurrentBag<object>>();
			_disposed = false;
		}

		private void TrackObject(object obj, RezolveContext context)
		{
			if (obj == null)
				return;
			ConcurrentBag<object> instances = _objects.GetOrAdd(
					new RezolveContext(null, context.RequestedType, context.Name),
					c => new ConcurrentBag<object>());

			//bit slow this, but hopefully there won't be loads of them...
			//note that there'll be a memory overhead with this, certainly in portable,
			//as under the hood the internal implementation realises the enumerable as an array
			//before returning its enumerator.
			if (!instances.Any(o => object.ReferenceEquals(o, obj)))
				instances.Add(obj);
		}

		protected override ILifetimeScopeRezolver CreateLifetimeScopeInstance()
		{
			//have to re-implement this because it binds to a different version of the constructor
			//than the base class does.
			return new CombinedLifetimeScopeRezolver(this);
		}
		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//interesting thing here - is how to handle nested scopes.  A nested lifetime Builder
			//should, in general, track objects it creates, and they should NOT be tracked by parent scopes.
			//however, limited-lifetime targets - i.e. scoped singletons - SHOULD be tracked by parent scopes,
			//and any child scopes that request the same object should receive the one created from the 
			//parent Builder.
			var toReturn = CreateLifetimeScopeInstance();
			_children.Add(toReturn);
			return toReturn;
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
				ILifetimeScopeRezolver child = null;
				while (_children.TryTake(out child))
				{
					child.Dispose();
				}

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
