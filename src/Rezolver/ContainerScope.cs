using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	public class ContainerScope : IContainerScope
	{
		/// <summary>
		/// Explicitly scoped objects can be a mixture of disposable and non-disposable objects
		/// </summary>
		private readonly ConcurrentDictionary<ResolveContext, Lazy<object>> _explicitlyScopedObjects
			= new ConcurrentDictionary<ResolveContext, Lazy<object>>();
		/// <summary>
		/// implicitly scoped objects will always be IDisposable
		/// </summary>
		private ConcurrentBag<IDisposable> _implicitlyScopedObjects
			= new ConcurrentBag<IDisposable>();
		private readonly LockedList<IContainerScope> _childScopes
			= new LockedList<IContainerScope>();

		private bool _disposed = false;
		private bool _disposing = false;

		public bool Disposed { get { return _disposed; } }

		private readonly IContainer _container;
		public IContainer Container
		{
			get
			{
				return _container ?? Parent.Container;
			}
		}

		public IContainerScope Parent
		{
			get;
		}


		/// <summary>
		/// Creates a new container that is a child of another.
		/// 
		/// The <see cref="Container"/> will be inherited from the <paramref name="parentScope"/>
		/// by default, unless it's overriden by <paramref name="containerOverride"/>.
		/// </summary>
		/// <param name="parentScope">Required - the parent scope</param>
		/// <param name="containerOverride">Optional - the container which should be used for resolve
		/// operations executed against this scope (note - all the resolve methods are declared as extension
		/// methods which mirror those present on <see cref="IContainer"/>.</param>
		public ContainerScope(IContainerScope parentScope, IContainer containerOverride = null)
		{
			parentScope.MustNotBeNull(nameof(parentScope));
			Parent = parentScope;
			if (containerOverride != null)
				_container = containerOverride;
		}

		/// <summary>
		/// Creates a new root scope tied to the given <paramref name="container"/>
		/// </summary>
		/// <param name="container"></param>
		public ContainerScope(IContainer container)
		{
			container.MustNotBeNull(nameof(container));

			_container = container;
		}

		public IContainerScope CreateScope()
		{
			var newScope = new ContainerScope(this);
			_childScopes.Add(newScope);
			return newScope;
		}

		public void ChildScopeDisposed(IContainerScope child)
		{
			if (!_disposing)
			{
				_childScopes.Remove(child);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			//have to cope with recursion below because of the complexity of some of the disposable
			//scenarios we might have to deal with.
			if (Disposed || _disposing) return;

			if (disposing)
			{
				_disposing = true;
				try
				{
					using (var listLock = _childScopes.Lock())
					{
						//dispose all child scopes first
						foreach (var scope in _childScopes)
						{
							scope.Dispose();
						}
						_childScopes.Clear();
					}

					//note that explicitly scoped objects might not actually be IDisposable :)
					var allExplicitObjects = _explicitlyScopedObjects.Values.ToArray()
						.Select(l => l.Value).OfType<IDisposable>();
					var allImplicitObjects = _implicitlyScopedObjects.ToArray();

					//clear the collections FIRST to prevent possible recursion
					_explicitlyScopedObjects.Clear();
					_implicitlyScopedObjects = new ConcurrentBag<IDisposable>();

					foreach (var obj in allExplicitObjects.Concat(allImplicitObjects))
					{
						obj.Dispose();
					}

					
				}
				finally
				{
					_disposed = true;
					_disposing = false;
				}
			}
		}

		object IContainerScope.Resolve(ResolveContext context, Func<ResolveContext, object> factory, ScopeActivationBehaviour behaviour)
		{
//#error in here, if you debug the current failing test, you'll see the problem is with (I think) the types not flowing correctly through the ResolveContext.
			if (Disposed) throw new ObjectDisposedException("ContainerScope", "This scope has been disposed");

			if (behaviour == ScopeActivationBehaviour.Explicit)
			{
				//use a lazily evaluated object which is bound to this resolve context to ensure only one instance is created
				return _explicitlyScopedObjects.GetOrAdd(context, k => new Lazy<object>(() => factory(k))).Value;
			}
			else if (behaviour == ScopeActivationBehaviour.Implicit)
			{
				var result = factory(context);
				if (result is IDisposable) _implicitlyScopedObjects.Add((IDisposable)result);
				return result;
			}
			return factory(context);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			object toReturn = null;
			Container.TryResolve(new ResolveContext(this, serviceType), out toReturn);
			return toReturn;
		}
	}
}
