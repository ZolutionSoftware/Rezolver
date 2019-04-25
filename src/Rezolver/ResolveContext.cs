﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>**Design notes**: benchmarking this as a class and a struct, and also with pooling
    /// at the container level, shows that the class version is fastest, even though it imposes a
    /// 32 byte overhead per call. The speed difference between using Rezolver and a hand-written
    /// 'new' operation decreases the bigger the object graph is.</remarks>
    public sealed class ResolveContext : IServiceProvider, IEquatable<ResolveContext>
    {
        private readonly Type _requestedType;
        private readonly ContainerScope _scope;

        /// <summary>
        /// Gets the type being requested from the container.
        /// </summary>
        public Type RequestedType { get => _requestedType; } 

        internal Container Container => _scope.Container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="requestedType">The type of object to be resolved from the container.</param>
        public ResolveContext(Container container, Type requestedType)
        {
            _scope = container._scope;
            _requestedType = requestedType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolveContext"/> class from the given scope.
        ///
        /// The <see cref="Container"/> is inherited from the scope's <see cref="ContainerScope.Container"/>.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="requestedType">The of object to be resolved from the container.</param>
        public ResolveContext(ContainerScope scope, Type requestedType)
        {
            _scope = scope;
            _requestedType = requestedType;
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
            return _scope.Resolve<TService>();
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
            return _scope.Resolve(serviceType);
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
            return newContainer.Resolve(new ResolveContext(new ContainerScopeProxy(_scope, newContainer), serviceType));
        }

        public IEnumerable ResolveMany(Type serviceType)
        {
            return (IEnumerable)_scope.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }

        public IEnumerable<TService> ResolveMany<TService>()
        {
            return _scope.Resolve<IEnumerable<TService>>();
        }

        public ResolveContext ChangeRequestedType(Type serviceType)
        {
            if (serviceType != _requestedType)
                return new ResolveContext(_scope, serviceType);

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
                return new ResolveContext(new ContainerScopeProxy(_scope, newContainer), _requestedType);

            return this;
        }

        /// <summary>
        /// Creates a new scope either through the <see cref="Scope"/> or, if that's null, then the <see cref="Container"/>.
        /// </summary>
        /// <remarks>This interface implementation is present for when an object wants to be able to inject a scope factory
        /// in order to create child scopes which are correctly parented either to another active scope or the container.</remarks>
        public ContainerScope CreateScope() => _scope.CreateScope();

        /// <summary>
        /// Implements the <see cref="object.GetHashCode"/> method using the hash code of
        /// the <see cref="RequestedType"/>
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => _requestedType.GetHashCode();

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"(Type: {_requestedType}, Scope: {_scope})";
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            return ((IServiceProvider)_scope).GetService(serviceType);
        }

        bool IEquatable<ResolveContext>.Equals(ResolveContext other)
        {
            // two contexts are equal if their requested type is the same
            return _requestedType == other._requestedType;
        }

        internal T ActivateImplicit_ThisScope<T>(T instance)
        {
            if (!_scope._canActivate)
                return instance;
            return _scope.ActivateImplicit(instance);
        }

        internal T ActivateImplicit_RootScope<T>(T instance)
        {
            if (!_scope._root._canActivate)
                return instance;
            return _scope._root.ActivateImplicit(instance);
        }

        internal T ActivateExplicit_ThisScope<T>(int targetId, Func<ResolveContext, T> instanceFactory)
        {
            if (!_scope._canActivate)
                throw new NotSupportedException("No scope explicitly created; either resolve from a new scope; or use ScopedContainer");
            return _scope.ActivateExplicit(this, targetId, instanceFactory);
        }

        internal T ActivateExplicit_RootScope<T>(int targetId, Func<ResolveContext, T> instanceFactory)
        {
            if (!_scope._root._canActivate)
                throw new NotSupportedException("Root scope cannot track instances; either resolve from a new scope; or use ScopedContainer");
            return _scope._root.ActivateExplicit(this, targetId, instanceFactory);
        }
    }
}
