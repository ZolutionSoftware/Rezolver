using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	public enum ScopeActivationBehaviour
	{
		/// <summary>
		/// implicitly scoped objects are only added to the scope 
		/// for the purposes of disposing when the scope is disposed
		/// </summary>
		Implicit = 0,
		/// <summary>
		/// explicitly scoped objects act like singletons in the current scope
		/// </summary>
		Explicit = 1,
		/// <summary>
		/// This means that no scope should be considered, even if there is one.
		/// </summary>
		None
	}
	public interface IContainerScope : IDisposable
	{
		/// <summary>
		/// If this scope has a parent scope, this is it.
		/// </summary>
		IContainerScope Parent { get; }
		/// <summary>
		/// The container that this scope is tied to.  All standard resolve operations
		/// should be made against this container to begin with.
		/// </summary>
		IContainer Container { get; }
		/// <summary>
		/// Called to create a child scope from this scope.
		/// </summary>
		/// <returns></returns>
		IContainerScope CreateScope();
		/// <summary>
		/// Called by child scopes when they are disposed to notify the parent that they 
		/// will no longer need to be disposed of when the parent is disposed.
		/// </summary>
		/// <param name="child"></param>
		void ChildScopeDisposed(IContainerScope child);
		/// <summary>
		/// Execute the given object factory within this scope.  Depending on the
		/// scoping behaviour passed, the object will either be resolved directly from the
		/// scope (i.e. existing objects contained within it) or obtained by executing
		/// the factory and optionally tracking the object if it's IDisposable.
		/// </summary>
		/// <param name="context">The resolve context - please note that the container
		/// that's present on this is the actual container that should be used to 
		/// resolve objects</param>
		/// <param name="factory">The factory to be executed</param>
		/// <param name="behaviour"></param>
		/// <returns></returns>
		/// <remarks>This function is the primary workhorse of all scopes.  Most importantly,
		/// the object produced from the factory DOES NOT have to come from this scope's
		/// <see cref="Container"/> - the implementing type simply has to ensure that 
		/// it tracks whatever object is ultimately returned; potentially returning
		/// a previously tracked object if <paramref name="behaviour"/> is 
		/// <see cref="ScopeActivationBehaviour.Explicit"/></remarks>
		object Resolve(ResolveContext context, Func<ResolveContext, object> factory, ScopeActivationBehaviour behaviour);
	}

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
		private readonly ConcurrentBag<IDisposable> _implicitlyScopedObjects
			= new ConcurrentBag<IDisposable>();
		private readonly ConcurrentBag<IContainerScope> _childScopes
			= new ConcurrentBag<IContainerScope>();

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
			if (!_disposing) {
#error Need to be able to remove scopes as they are disposed but we can't because ConcurrentBag doesn't support that.
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (Disposed) return;

			if (disposing)
			{
				_disposing = true;
				try
				{
					//dispose all child scopes first
					var childScopes = _childScopes.ToArray();

					foreach (var scope in childScopes)
					{
						scope.Dispose();
					}
					//note that explicitly scoped objects might not actually be IDisposable :)
					var allExplicitObjects = _explicitlyScopedObjects.Values.ToArray()
						.Select(l => l.Value).OfType<IDisposable>();
					var allImplicitObjects = _implicitlyScopedObjects.ToArray();

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

		public object Resolve(ResolveContext context, Func<ResolveContext, object> factory, ScopeActivationBehaviour behaviour)
		{
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

	}

	public static class IContainerScopeExtensions
	{
		public static TResult Resolve<TResult>(this IContainerScope scope, ResolveContext context, Func<ResolveContext, object> factory, ScopeActivationBehaviour behaviour)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			return (TResult)scope.Resolve(context, factory, behaviour);
		}
		/// <summary>
		/// Resolves an object within the current scope.
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="scope"></param>
		/// <returns></returns>
		/// <remarks>Resolving an object via a scope does not guarantee that it will be
		/// tracked.  Ultimately, it's up to the behaviour of the individual underlying
		/// targets to determine whether they should interact with the scope.
		/// 
		/// Indeed, all this extension method does is to forward the method call on to the
		/// <see cref="IContainerScope.Container"/> of the given scope, ensuring that
		/// the scope is set on the <see cref="ResolveContext"/> that is passed to its
		/// <see cref="IContainer.Resolve(ResolveContext)"/> method.
		/// 
		/// It is then the responsibility of the target compiler to ensure that targets
		/// interact with the active scope correctly.
		/// 
		/// All the scope implementation has to do is ensure that it tracks objects correctly
		/// according to their scope activation rules.</remarks>
		public static TResult Resolve<TResult>(this IContainerScope scope)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			return (TResult)scope.Container.Resolve(new ResolveContext(scope, typeof(TResult)));
		}
	}
}
