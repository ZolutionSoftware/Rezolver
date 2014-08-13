using System;
using System.Collections.Generic;
using System.Threading;

namespace Rezolver
{
	public class LifetimeScopeRezolver : CachingRezolver, ILifetimeScopeRezolver
	{
		private List<ILifetimeScopeRezolver> _children;
		private Dictionary<RezolverKey, List<IDisposable>> _disposables;

		public override IRezolveTargetCompiler Compiler
		{
			get
			{
				return _parent.Compiler;
			}
		}

		protected override IRezolverBuilder Builder
		{
			get
			{
				return _parent;
			}
		}

		private bool _disposing;
		private bool _disposed;
		private readonly IRezolver _parent;
		public LifetimeScopeRezolver(IRezolver parent)
		{
			_children = new List<ILifetimeScopeRezolver>();
			_parent = parent;
			_disposables = new Dictionary<RezolverKey, List<IDisposable>>();
			_disposing = _disposed = false;
		}

		public override object Resolve(Type type, string name = null, IRezolver dynamicRezolver = null)
		{
			var result = _parent.Resolve(type, name, dynamicRezolver);
			//I think targets need to compile a special version of their code which
			//accepts a lifetime Builder (and optionally a dynamically supplied rezolver), so that
			//the target itself has full control over how it creates its object under
			//lifetime scopes (and whether the object gets added to them).  This method would 
			//then be implemented similarly to how the base method is done - i.e. fetching 
			//the compiled target and executing it.

			//This would allow for the implementation of a scoped singleton, but 
			//the implementation would not be trivial, as it would need to query
			//the incoming scope to see if an instance already existed.
			//This will mean an extension of the ILifetimeScopeRezolver interface
			//to support explicit registration and fetching of instances - in parallel
			//to the basic functionality of resolving new instances.
			TrackDisposable(new RezolverKey(type, name), result as IDisposable);
			return result;
		}

		private void TrackDisposable(RezolverKey rezolverKey, IDisposable obj)
		{
			if (obj == null)
				return;
			List<IDisposable> instanceList = null;
			if (!_disposables.TryGetValue(rezolverKey, out instanceList))
				_disposables[rezolverKey] = instanceList = new List<IDisposable>();

			instanceList.Add(obj);
		}

		public override T Resolve<T>(string name = null, IRezolver @dynamic = null)
		{
			return _parent.Resolve<T>(name, dynamic);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//interesting thing here - is how to handle nested scopes.  A nested lifetime Builder
			//should, in general, track objects it creates, and they should NOT be tracked by parent scopes.
			//however, limited-lifetime targets - i.e. scoped singletons - SHOULD be tracked by parent scopes,
			//and any child scopes that request the same object should receive the one created from the 
			//parent Builder.
			var toReturn = new LifetimeScopeRezolver(_parent);
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

			foreach (var kvp in _disposables)
			{
				foreach (var disposable in kvp.Value)
				{
					disposable.Dispose();
				}
			}
			_disposed = true;
			_disposing = false;
		}

		public void AddToScope(IDisposable disposable)
		{
			disposable.MustNotBeNull("disposable");
			TrackDisposable(new RezolverKey(disposable.GetType(), null), disposable);
		}
	}
}