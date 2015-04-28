using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Rezolver
{
	//TODO: reimplement this as a combined rezolver - there's no need to override caching rezolver any more.
	public class LifetimeScopeRezolver : CombinedRezolver, ILifetimeScopeRezolver
	{
		private List<ILifetimeScopeRezolver> _children;
		private Dictionary<RezolveContext, List<object>> _objects;

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				return _parentScope;
			}
		}

		private bool _disposing;
		private bool _disposed;
		private readonly ILifetimeScopeRezolver _parentScope;
		public LifetimeScopeRezolver(IRezolver inner, IRezolverBuilder builder = null, IRezolveTargetCompiler compiler = null) 
			: base(inner, builder, compiler)
		{
			//TODO: add the ability to specify that a GC is to be performed
			//when this scope is disposed?
			_children = new List<ILifetimeScopeRezolver>();
			_objects = new Dictionary<RezolveContext, List<object>>();
			_disposing = _disposed = false;
		}
		public LifetimeScopeRezolver(ILifetimeScopeRezolver parentScope, IRezolver rezolver = null)
			: this(rezolver ?? (IRezolver)parentScope)
		{
			_parentScope = parentScope;
		}

		public override object Resolve(RezolveContext context)
		{
			if (context.Scope == null)
				context = context.CreateNew(this); //ensure this scope is added to the context
			var result = base.Resolve(context);
			//if the object is destined for this scope, then track it.
			if(result is IDisposable && object.ReferenceEquals(this, context.Scope))
				TrackObject(result, context);
			return result;
		}

		private void TrackObject(object obj, RezolveContext context)
		{
			if (obj == null)
				return;
			List<object> instanceList = null;
			if (!_objects.TryGetValue(context, out instanceList))
			{
				var keyContext = new RezolveContext(null, context.RequestedType, context.Name);
				_objects[keyContext] = instanceList = new List<object>();
			}
			//bit slow this, but hopefully there won't be loads of them...
			if(!instanceList.Any(o => object.ReferenceEquals(o, obj)))
				instanceList.Add(obj);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//interesting thing here - is how to handle nested scopes.  A nested lifetime Builder
			//should, in general, track objects it creates, and they should NOT be tracked by parent scopes.
			//however, limited-lifetime targets - i.e. scoped singletons - SHOULD be tracked by parent scopes,
			//and any child scopes that request the same object should receive the one created from the 
			//parent Builder.
			var toReturn = new LifetimeScopeRezolver(this);
			_children.Add(toReturn);
			return toReturn;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification="The object has no finalizer")]
		public virtual void Dispose()
		{
			if (_disposed || _disposing)
				return;
			_disposing = true;
			foreach (var child in _children)
			{
				child.Dispose();
			}

			foreach (var kvp in _objects)
			{
				foreach (var disposable in kvp.Value.OfType<IDisposable>())
				{
					disposable.Dispose();
				}
			}
			_children.Clear();
			_objects.Clear();
			_disposed = true;
			_disposing = false;
		}

		public void AddToScope(object obj, RezolveContext context = null)
		{
			obj.MustNotBeNull("obj");
			TrackObject(obj, context ?? new RezolveContext(null, obj.GetType()));
		}


		public IEnumerable<object> GetFromScope(RezolveContext context)
		{
			context.MustNotBeNull("context");
			List<object> instanceList = null;
			if (_objects.TryGetValue(context, out instanceList))
			{
				//important to return a read-only collection here to avoid modification
				return new ReadOnlyCollection<object>(instanceList);
			}
			else
				return Enumerable.Empty<object>();
		}
	}

	
}