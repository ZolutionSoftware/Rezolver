// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// Base class for scopes.
    /// 
    /// This type cannot be inherited by user types.
    /// </summary>
    public class ContainerScope : IDisposable, IServiceProvider
    {
        private protected bool _isDisposing = false;
        private protected bool _isDisposed = false;
        internal bool _canActivate = false;
        internal readonly ContainerScope _root;
        private readonly Container _container;
        private readonly ContainerScope _parent;

        /// <summary>
        /// The container that this scope uses by default to resolve instances.
        /// </summary>
        public Container Container => _container;
        /// <summary>
        /// Root scope to be used for 'top-level' object tracking.
        /// 
        /// Note - when a <see cref="Container"/> is used, this should be the first scope
        /// created via the <see cref="Container.CreateScope"/> method.
        /// 
        /// With <see cref="ScopedContainer"/> it will be the scope that lives inside that container.
        /// 
        /// So, as a result of this, this property could point to a scope that's not actually
        /// a parent of <see cref="Parent"/>
        /// </summary>
        public ContainerScope Root => _root;
        /// <summary>
        /// The scope from which this scope was created.
        /// </summary>
        public ContainerScope Parent => _parent;
        /// <summary>
        /// Creates a new Root scope whose container is set to <paramref name="container"/>
        /// </summary>
        /// <param name="container"></param>
        private protected ContainerScope(Container container)
        {
            _container = container;
            _root = this;
        }

        /// <summary>
        /// Creates a new child scope whose container is inherited from the <paramref name="parent"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isRoot"></param>
        private protected ContainerScope(ContainerScope parent, bool isRoot)
        {
            _container = parent._container;
            _parent = parent;
            // cheeky - basically as soon as we have a fully-functioning scope that can track instances,
            // that must become the root scope; but until that point, it just gets 
            _root = isRoot ? this : parent._root;
        }

        internal virtual T ActivateImplicit<T>(T instance)
        {
            throw new NotSupportedException($"This scope (type = {this.GetType()}) cannot track instances, either manually create a scope, or use the {nameof(ScopedContainer)} as your container type");
        }

        internal virtual T ActivateExplicit<T>(ResolveContext context, int targetId, Func<ResolveContext, T> instanceFactory)
        {
            throw new NotSupportedException($"Cannot create instance of {context.RequestedType} from target #{targetId} - Explicitly scoped objects are not supported by the default {nameof(ContainerScope)} - either manually create a scope, or use the {nameof(ScopedContainer)} as your container type");
        }

        /// <summary>
        /// Creates a new scope from which instances can be resolved, isolated from the current scope.
        /// </summary>
        /// <returns>A new scope.</returns>
        public virtual ContainerScope CreateScope()
        {
            // note that the new scope is set as its own root if this scope's Root is not 
            // a 'full' instance-tracking scope.
            return new DisposingContainerScope(this, !(this._root is DisposingContainerScope));
        }

        /// <summary>
        /// Disposes this scope and any of its child scopes.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_isDisposed && !_isDisposing)
                {
                    _isDisposing = true;
                }

                try
                {
                    // allow derived types to dispose any objects they're tracking.
                    OnDispose();
                }
                finally
                {
                    _isDisposing = false;
                    _isDisposed = true;
                }
                _parent?.ChildDisposed(this);
            }
        }

        private protected virtual void OnDispose()
        {

        }

        object IServiceProvider.GetService(Type serviceType)
        {
            // when resolving through a scope, we give the container a context
            // which explicitly has this scope on it.
            _container.TryResolve(new ResolveContext(this, serviceType), out object result);
            return result;
        }

        /// <summary>
        /// Resolves an instance of the <paramref name="serviceType"/> from the container.
        /// </summary>
        /// <param name="serviceType">The type of object to resolve.</param>
        /// <returns>An instance of the type <paramref name="serviceType"/> built according to the 
        /// registrations in this container.</returns>
        public object Resolve(Type serviceType)
        {
            return _container.Resolve(new ResolveContext(this, serviceType));
        }

        /// <summary>
        /// Resolves an instance of <typeparamref name="TService"/> from the container.
        /// </summary>
        /// <typeparam name="TService">The type of object to resolve.</typeparam>
        /// <returns>An instance of the type <typeparamref name="TService"/> built according to the
        /// registrations in this container.</returns>
        public TService Resolve<TService>()
        {
            return _container.ResolveInternal<TService>(new ResolveContext(this, typeof(TService)));
        }

        /// <summary>
        /// Resolves an enumerable of zero or more instances of <paramref name="serviceType"/> from the
        /// container.
        /// </summary>
        /// <param name="serviceType">Type of objects to resolve.</param>
        /// <returns>An enumerable containing zero or more instances of the type <paramref name="serviceType"/>.</returns>
        public IEnumerable ResolveMany(Type serviceType)
        {
            return (IEnumerable)_container.Resolve(new ResolveContext(this, typeof(IEnumerable<>).MakeGenericType(serviceType)));
        }

        /// <summary>
        /// Resolves an enumerable of zero or more instances of <typeparamref name="TService"/> from the
        /// container.
        /// </summary>
        /// <typeparam name="TService">Type of objects to resolve.</typeparam>
        /// <returns>An enumerable containing zero or more instances of the type <typeparamref name="TService"/>.</returns>
        public IEnumerable<TService> ResolveMany<TService>()
        {
            return _container.ResolveInternal<IEnumerable<TService>>(new ResolveContext(this, typeof(IEnumerable<TService>)));
        }

        private protected virtual void ChildDisposed(ContainerScope child)
        {

        }
    }

    /// <summary>
    /// Default scope object used by the<see cref="Container"/> - performs no actual lifetime management
    /// of objects at all, but can create child scopes and track them for disposal when it is disposed.
    /// 
    /// In order to get object lifetime management from a container whose scope is set to an instance of this type,
    /// a new scope must be created from this one.
    /// </summary>
    public class NonTrackingContainerScope : ContainerScope
    {
        private LockedList<ContainerScope> _childScopes
            = new LockedList<ContainerScope>(64);

        internal NonTrackingContainerScope(Container container)
            : base(container)
        {

        }

        internal NonTrackingContainerScope(ContainerScope parent, bool isRoot)
            : base(parent, isRoot)
        {

        }

        private protected override void OnDispose()
        {
            using (var listLock = this._childScopes.Lock())
            {
                // dispose child scopes in reverse order of creation
                for (var f = this._childScopes.Count; f > 0; f--)
                {
                    this._childScopes[f - 1].Dispose();
                }
            }

            this._childScopes = null;
        }

        private protected override void ChildDisposed(ContainerScope child)
        {
            if (!_isDisposing && !_isDisposed)
            {
                this._childScopes.Remove(child);
            }
        }

        /// <summary>
        /// Creates a new child scope from this scope.  The new scope will use the same container as this one,
        /// but will have its own lifetime, and will track its own instances of any 'scoped' objects.
        /// 
        /// When this scope is disposed, the new scope will be disposed also - unless it has already been disposed.
        /// </summary>
        /// <returns></returns>
        public sealed override ContainerScope CreateScope()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(NonTrackingContainerScope));

            var scope = base.CreateScope();
            this._childScopes.Add(scope);
            return scope;
        }
    }

    /// <summary>
    /// Fully functional container scope.  Supports tracking of implicitly and explicitly scoped objects to enable disposal.
    /// </summary>
    public sealed class DisposingContainerScope : NonTrackingContainerScope
    {
        // TODO: change this to use a different 'key' which includes the factory
        // so that we don't need a closure to create the lazy.

        /// <summary>
        /// Explicitly scoped objects can be a mixture of disposable and non-disposable objects
        /// </summary>
        private ConcurrentDictionary<TypeAndTargetId, Lazy<ScopedObject>> _explicitlyScopedObjects
            = new ConcurrentDictionary<TypeAndTargetId, Lazy<ScopedObject>>();

        /// <summary>
        /// implicitly scoped objects will always be IDisposable
        /// </summary>
        private ConcurrentBag<ScopedObject> _implicitlyScopedObjects
            = new ConcurrentBag<ScopedObject>();

        private readonly struct ScopedObject
        {
            private static int _order = 0;

            public int Id { get; }

            public object Object { get; }

            public ScopedObject(object obj)
            {
                Id = _order++;
                Object = obj;
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DisposingContainerScope"/> class.
        /// </summary>
        /// <param name="container">The container that this scope will use to resolve instances.</param>
        public DisposingContainerScope(Container container)
            : base(container)
        {
            _canActivate = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DisposingContainerScope"/> class that
        /// is optionally treated as a root scope.  Typically, root scopes belong to containers
        /// so that there is always a scope on-hand.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isRoot"></param>
        public DisposingContainerScope(ContainerScope parent, bool isRoot)
            : base(parent, isRoot)
        {
            _canActivate = true;
        }

        internal override T ActivateImplicit<T>(T instance)
        {
            if (instance is IDisposable)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(DisposingContainerScope));

                this._implicitlyScopedObjects.Add(new ScopedObject(instance));
            }

            return instance;
        }

        internal override T ActivateExplicit<T>(ResolveContext context, int targetId, Func<ResolveContext, T> instanceFactory)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(DisposingContainerScope));

            var key = new TypeAndTargetId(typeof(T), targetId);
            // TODO: RequestedType is IEnumerable<Blah> when a scoped object is requested as part of an enumerable - hence why these two MSDI Tests fail.
            if (this._explicitlyScopedObjects.TryGetValue(key, out Lazy<ScopedObject> lazy))
            {
                return (T)lazy.Value.Object;
            }

            // use a lazily evaluated object which is bound to this resolve context to ensure only one instance is created
            return (T)this._explicitlyScopedObjects.GetOrAdd(key, CreateLazy).Value.Object;

            Lazy<ScopedObject> CreateLazy(TypeAndTargetId k) => new Lazy<ScopedObject>(CreateScoped);

            ScopedObject CreateScoped() => new ScopedObject(instanceFactory(context));
        }

        private protected override void OnDispose()
        {
            try
            {
                base.OnDispose();
            }
            finally
            {
                // note that explicitly scoped objects might not actually be IDisposable :)
                var allExplicitObjects = this._explicitlyScopedObjects.Skip(0).ToArray()
                    .Where(l => l.Value.Value.Object is IDisposable && !(l.Value.Value.Object is ContainerScope))
                    .Select(l => l.Value.Value);
                var allImplicitObjects = this._implicitlyScopedObjects.Skip(0).ToArray()
                    .Where(l => !(l.Object is ContainerScope));

                // deref all used collections
                this._explicitlyScopedObjects = null;
                this._implicitlyScopedObjects = null;

                foreach (var obj in allExplicitObjects.Concat(allImplicitObjects)
                    .OrderByDescending(so => so.Id))
                {
                    ((IDisposable)obj.Object).Dispose();
                }
            }
        }
    }

    /// <summary>
    /// A scope which proxies another, but with a different Container
    /// </summary>
    internal sealed class ContainerScopeProxy : ContainerScope
    {
        private readonly ContainerScope _inner;
        public ContainerScopeProxy(ContainerScope inner, Container newContainer)
            : base(newContainer)
        {
            _inner = inner;
            _canActivate = inner._canActivate;
        }

        internal override T ActivateImplicit<T>(T instance)
        {
            return _inner.ActivateImplicit(instance);
        }

        internal override T ActivateExplicit<T>(ResolveContext context, int targetId, Func<ResolveContext, T> instanceFactory)
        {
            return _inner.ActivateExplicit(context, targetId, instanceFactory);
        }
    }
}
