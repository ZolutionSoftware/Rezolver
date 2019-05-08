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

        /// <summary>
        /// The container that this scope uses by default to resolve instances.
        /// </summary>
        public Container Container { get; private set; }

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
        public ContainerScope Parent { get; private set; }

        /// <summary>
        /// Creates a new Root scope whose container is set to <paramref name="container"/>
        /// </summary>
        /// <param name="container"></param>
        private protected ContainerScope(Container container)
        {
            Container = container;
            _root = this;
        }

        /// <summary>
        /// Creates a new child scope whose container is inherited from the <paramref name="parent"/>
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="isRoot"></param>
        private protected ContainerScope(ContainerScope parent, bool isRoot)
        {
            Container = parent.Container;
            Parent = parent;
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
                Parent?.ChildDisposed(this);
            }
        }

        private protected virtual void OnDispose()
        {

        }

        object IServiceProvider.GetService(Type serviceType)
        {
            // when resolving through a scope, we give the container a context
            // which explicitly has this scope on it.
            Container.TryResolve(new ResolveContext(this, serviceType), out object result);
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
            return Container.Resolve(new ResolveContext(this, serviceType));
        }

        /// <summary>
        /// Resolves an instance of <typeparamref name="TService"/> from the container.
        /// </summary>
        /// <typeparam name="TService">The type of object to resolve.</typeparam>
        /// <returns>An instance of the type <typeparamref name="TService"/> built according to the
        /// registrations in this container.</returns>
        public TService Resolve<TService>()
        {
            return Container.ResolveInternal<TService>(new ResolveContext(this, typeof(TService)));
        }

        /// <summary>
        /// Resolves an enumerable of zero or more instances of <paramref name="serviceType"/> from the
        /// container.
        /// </summary>
        /// <param name="serviceType">Type of objects to resolve.</param>
        /// <returns>An enumerable containing zero or more instances of the type <paramref name="serviceType"/>.</returns>
        public IEnumerable ResolveMany(Type serviceType)
        {
            return (IEnumerable)Container.Resolve(new ResolveContext(this, typeof(IEnumerable<>).MakeGenericType(serviceType)));
        }

        /// <summary>
        /// Resolves an enumerable of zero or more instances of <typeparamref name="TService"/> from the
        /// container.
        /// </summary>
        /// <typeparam name="TService">Type of objects to resolve.</typeparam>
        /// <returns>An enumerable containing zero or more instances of the type <typeparamref name="TService"/>.</returns>
        public IEnumerable<TService> ResolveMany<TService>()
        {
            return Container.ResolveInternal<IEnumerable<TService>>(new ResolveContext(this, typeof(IEnumerable<TService>)));
        }

        private protected virtual void ChildDisposed(ContainerScope child)
        {

        }
    }
}
