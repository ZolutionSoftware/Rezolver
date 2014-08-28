using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Rezolver
{
	public class LifetimeScopeRezolver : CachingRezolver, ILifetimeScopeRezolver
	{
		private List<ILifetimeScopeRezolver> _children;
		private Dictionary<RezolveContext, List<IDisposable>> _disposables;

		public ILifetimeScopeRezolver ParentScope
		{
			get
			{
				return _parentScope;
			}
		}

		public override IRezolveTargetCompiler Compiler
		{
			get
			{
				return _rezolver.Compiler;
			}
		}

		protected override IRezolverBuilder Builder
		{
			get
			{
				return _rezolver;
			}
		}

		private bool _disposing;
		private bool _disposed;
		private readonly IRezolver _rezolver;
		private readonly ILifetimeScopeRezolver _parentScope;
		public LifetimeScopeRezolver(IRezolver rezolver)
		{
			_children = new List<ILifetimeScopeRezolver>();
			_rezolver = rezolver;
			_disposables = new Dictionary<RezolveContext, List<IDisposable>>();
			_disposing = _disposed = false;
		}
		public LifetimeScopeRezolver(ILifetimeScopeRezolver parentScope, IRezolver rezolver = null)
			: this(rezolver ?? (IRezolver)parentScope)
		{
			_parentScope = parentScope;
		}

		public override object Resolve(RezolveContext context)
		{
			var result = base.Resolve(context);
			TrackDisposable(result as IDisposable, context);
			return result;
		}

		public override T Resolve<T>(RezolveContext context)
		{
			var result = base.Resolve<T>(context);
			TrackDisposable(result as IDisposable, context);
			return result;
		}

		private void TrackDisposable(IDisposable obj, RezolveContext context)
		{
			if (obj == null)
				return;
			List<IDisposable> instanceList = null;
			if (!_disposables.TryGetValue(context, out instanceList))
			{
				var keyContext = new RezolveContext(context.RequestedType, context.Name);
				_disposables[keyContext] = instanceList = new List<IDisposable>();
			}

			instanceList.Add(obj);
		}

		public override ILifetimeScopeRezolver CreateLifetimeScope()
		{
			//interesting thing here - is how to handle nested scopes.  A nested lifetime Builder
			//should, in general, track objects it creates, and they should NOT be tracked by parent scopes.
			//however, limited-lifetime targets - i.e. scoped singletons - SHOULD be tracked by parent scopes,
			//and any child scopes that request the same object should receive the one created from the 
			//parent Builder.
			var toReturn = new LifetimeScopeRezolver(this, _rezolver);
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

		public void AddToScope(IDisposable disposable, RezolveContext context = null)
		{
			disposable.MustNotBeNull("disposable");
			TrackDisposable(disposable, context ?? new RezolveContext(disposable.GetType()));
		}


		public IEnumerable<IDisposable> GetFromScope(RezolveContext context)
		{
			context.MustNotBeNull("context");
			List<IDisposable> instanceList = null;
			_disposables.TryGetValue(context, out instanceList);

			//important to return a read-only collection here to avoid modification
			return new ReadOnlyCollection<IDisposable>(instanceList);
		}
	}
}