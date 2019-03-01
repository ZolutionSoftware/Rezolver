// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;

namespace Rezolver
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ResolveContext : IServiceProvider
    {
        /// <summary>
        /// Gets the type being requested from the container.
        /// </summary>
        public Type RequestedType { get; private set; }

        /// <summary>
        /// The container for this context.
        /// </summary>
        /// <remarks>This is a wrapper for the <see cref="ContainerScope2.Container"/> property of the <see cref="Scope"/></remarks>
        public Container Container => Scope.Container;

        /// <summary>
        /// Gets the scope that's active for all calls for this context.
        /// </summary>
        /// <value>The scope.</value>
        public ContainerScope2 Scope { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="requestedType">The type of object to be resolved from the container.</param>
        public ResolveContext(Container container, Type requestedType)
        {
            Scope = container.Scope;
            RequestedType = requestedType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class from the given scope.
        ///
        /// The <see cref="Container"/> is inherited from the scope's <see cref="IContainerScope.Container"/>.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="requestedType">The of object to be resolved from the container.</param>
        public ResolveContext(ContainerScope2 scope, Type requestedType)
        {
            Scope = scope;
            RequestedType = requestedType;
        }

        /*****
         * Resolve operations on the ResolveContext don't allow for changing the scope.
         * You can request a different type, optionally from a different container, but
         * the scope is fixed.
         *****/

        /// <summary>
        /// Resolves a new instance of a different type from the same scope & container that originally
        /// received the current Resolve operation.
        /// </summary>
        /// <typeparam name="TService">Type of service to be resolved.</typeparam>
        public TService Resolve<TService>()
        {
            return Scope.Resolve<TService>();
        }

        public TService Resolve<TService>(Container newContainer)
        {
            return newContainer.ResolveInternal<TService>(ChangeContainer(newContainer));
        }

        /// <summary>
        /// Resolves a new instance of a different type from the same scope & container that's
        /// on this context.
        /// </summary>
        /// <param name="serviceType">Type of service to be resolved.</param>
        public object Resolve(Type serviceType)
        {
            return Scope.Resolve(serviceType);
        }

        /// <summary>
        /// Resolves a new instance of a different type from a different container
        /// than the one on the <see cref="Scope"/> of this context.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="newContainer"></param>
        /// <returns></returns>
        public object Resolve(Type serviceType, Container newContainer)
        {
            // switching contains means creating a scope proxy which creates a temporary association between the 
            // scope and the new container.
            return newContainer.Resolve(new ResolveContext(new ContainerScopeProxy(Scope, newContainer), serviceType));
        }

        public IEnumerable ResolveMany(Type serviceType)
        {
            return (IEnumerable)Scope.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }

        public IEnumerable<TService> ResolveMany<TService>()
        {
            return Scope.Resolve<IEnumerable<TService>>();
        }

        public ResolveContext ChangeRequestedType(Type serviceType)
        {
            if (serviceType != RequestedType)
                return new ResolveContext(Scope, serviceType);

            return this;
        }

        /// <summary>
        /// Creates a new context with the same <see cref="RequestedType"/> but which binds the current 
        /// <see cref="Scope"/> to a different <paramref name="newContainer"/>.  So, if an instance is
        /// resolved, it will be tracked in this scope even though the container might be different.
        /// </summary>
        /// <param name="newContainer"></param>
        /// <returns></returns>
        public ResolveContext ChangeContainer(Container newContainer)
        {
            // we have to create a new 'fake' scope which proxies the current one,
            // but with a different container.
            if (newContainer != Container)
                return new ResolveContext(new ContainerScopeProxy(Scope, newContainer), RequestedType);

            return this;
        }

        /// <summary>
        /// Creates a new scope either through the <see cref="Scope"/> or, if that's null, then the <see cref="Container"/>.
        /// </summary>
        /// <remarks>This interface implementation is present for when an object wants to be able to inject a scope factory
        /// in order to create child scopes which are correctly parented either to another active scope or the container.</remarks>
        public ContainerScope2 CreateScope()
        {
            return Scope.CreateScope();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"(Type: {RequestedType}, Scope: {Scope})";
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IServiceProvider)Scope).GetService(serviceType);
        }
    }
}
