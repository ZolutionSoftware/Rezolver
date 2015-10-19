using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Collections.Concurrent;

namespace Rezolver
{
	public class CombinedLifetimeScopeRezolver : CombinedRezolver, ILifetimeScopeRezolver
	{
		private ConcurrentBag<ILifetimeScopeRezolver> _children;
		private ConcurrentDictionary<RezolveContext, ConcurrentBag<object>> _objects;

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				return _parentScope;
			}
		}

		private bool _disposed;
		private readonly ILifetimeScopeRezolver _parentScope;
		public CombinedLifetimeScopeRezolver(IRezolver inner, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
				: this(null, inner, builder: builder, compiler: compiler)
		{ 
			
		}

		public CombinedLifetimeScopeRezolver(ILifetimeScopeRezolver parentScope, IRezolver inner = null, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null)
				: base(inner ?? parentScope, builder: builder, compiler: compiler)

		{
			_parentScope = parentScope;
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

		/// <summary>
		/// Have to re-implement this method because it binds to a different version of the constructor (for the moment)
		/// </summary>
		/// <returns></returns>
		protected override ILifetimeScopeRezolver CreateLifetimeScopeInstance()
		{
			return new CombinedLifetimeScopeRezolver(this);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
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