﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using Rezolver.Events;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainer"/> which can override the resolve operations of another.  This is useful when you have a
    /// core application-wide container, with some objects being customised based on some ambient information,
    /// e.g. configuration, MVC Area/Controller, Brand (in a multi-tenant application for example) or more.
    /// </summary>
    /// <remarks>When overriding another <see cref="IContainer"/>, you are overriding the <see cref="ICompiledTarget"/> objects that
    /// will be returned when <see cref="IContainer.GetCompiledTarget(ResolveContext)"/> is called on that container and, therefore,
    /// the compiled target which is executed when the <see cref="IContainer.Resolve(ResolveContext)"/> method is called.
    ///
    /// This has the side effect of overriding automatically resolved arguments (bound to a <see cref="ResolvedTarget"/>) compiled
    /// in the overridden container by virtue of the fact that the overriding container is a different reference, because the
    /// <see cref="ResolvedTarget"/> is typically compiled with a check, at resolve-time, that the
    /// <see cref="ResolveContext.Container"/> is the same container as the one that was active when it was originally compiled.
    ///
    /// In essence, when resolving an instance as a dependency the <see cref="ResolvedTarget"/> does something like this:
    ///
    /// <code>resolveContext.Container == compileContext.Container ? (execute compile-time target) : resolveContext.Container.Resolve(type)</code>
    ///
    /// ---
    ///
    /// ## Behaviour of enumerables
    ///
    /// The behaviour of enumerables depends on whether enumerables are enabled via the <see cref="Options.EnableEnumerableInjection"/> option
    /// (<c>true</c> by default) and whether the <see cref="Configuration.OverridingEnumerables"/> is applied to the container
    /// (which it is, also by default, via the <see cref="Container.DefaultConfig"/>).
    ///
    /// If so, then any request for <c>IEnumerable&lt;T&gt;</c> will result in an enumerable which combines the one resolved by the <see cref="Inner"/>
    /// container, and then by any direct registration in this container.  This mirrors the behaviour of the <see cref="ITargetContainer.FetchAll(Type)"/>
    /// implementation of the <see cref="OverridingTargetContainer"/> - which produces an enumerable of targets from both the base target container and
    /// the overriding one.
    ///
    /// If the <see cref="Configuration.OverridingEnumerables"/> is not applied to this container, then any IEnumerable registered (or automatically built)
    /// in this container will override that of the <see cref="Inner"/> container.
    /// </remarks>
    public sealed class OverridingContainer : Container
    {
        private class CombinedTargetContainer : IRootTargetContainer
        {
            private readonly IRootTargetContainer _first;
            private readonly IRootTargetContainer _second;

            public IRootTargetContainer Root { get; }

            public event EventHandler<TargetRegisteredEventArgs> TargetRegistered;
            public event EventHandler<TargetContainerRegisteredEventArgs> TargetContainerRegistered;

            public CombinedTargetContainer(IRootTargetContainer first, IRootTargetContainer second)
            {
                this._first = first;
                this._second = second;
            }

            public void AddKnownType(Type serviceType)
            {
                _second.AddKnownType(serviceType);
            }

            public ITargetContainer CombineWith(ITargetContainer existing, Type type)
            {
                return _second.CombineWith(existing, type);
            }

            public ITargetContainer CreateTargetContainer(Type forContainerRegistrationType)
            {
                return _second.CreateTargetContainer(forContainerRegistrationType);
            }

            public ITarget Fetch(Type type)
            {
                var result = _second.Fetch(type);
                if (result?.UseFallback ?? true)
                    return _first.Fetch(type);
                return result;
            }

            public IEnumerable<ITarget> FetchAll(Type type)
            {
                return _first.FetchAll(type).Concat(_second.FetchAll(type));
            }

            public ITargetContainer FetchContainer(Type type)
            {
                return _second.FetchContainer(type);
            }

            public Type GetContainerRegistrationType(Type serviceType)
            {
                return GetContainerRegistrationType(serviceType);
            }

            public IEnumerable<Type> GetKnownCompatibleTypes(Type serviceType)
            {
                //return
            }

            public IEnumerable<Type> GetKnownCovariantTypes(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public void Register(ITarget target, Type serviceType = null)
            {
                throw new NotImplementedException();
            }

            public void RegisterContainer(Type type, ITargetContainer container)
            {
                throw new NotImplementedException();
            }

            public TargetTypeSelector SelectTypes(Type type)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> that is overriden by this container.
        /// </summary>
        //public Container Inner { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="OverridingContainer"/>
        /// </summary>
        /// <param name="inner">Required.  The inner container that this one combines with.  Any dependencies not served
        /// by the new combined container's own targets will be sought from this container.  Equally, any targets in the base which
        /// are resolved when the overriding container is the root container for a resolve operation, will resolve
        /// their dependencies from this container.</param>
        /// <param name="targets">Optional. Contains the targets that will be used as the source of registrations for this container
        /// (note - separate to those of the <paramref name="inner"/> container).
        ///
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <param name="config">Can be null.  A configuration to apply to this container (and, potentially its
        /// <see cref="Targets"/>).  If not provided, then the <see cref="Container.DefaultConfig"/> will be used</param>
        public OverridingContainer(Container inner, IRootTargetContainer targets = null, IContainerConfig config = null)
            : base(new OverridingTargetContainer(inner))
        {
            //Inner = inner ?? throw new ArgumentNullException(nameof(inner));

            (config ?? DefaultConfig).Configure(this, Targets);
        }

        ///// <summary>
        ///// Overrides the base method to check if both this and the inner container can resolve the type.
        ///// </summary>
        ///// <param name="serviceType"></param>
        ///// <returns></returns>
        //public sealed override bool CanResolve(Type serviceType)
        //{
        //    return base.CanResolve(serviceType) || Inner.CanResolve(serviceType);
        //}

        ///// <summary>
        ///// Overrides the base implementation to pass the lookup for an <see cref="ITarget"/> to the inner container - this
        ///// is how dependency chaining from this container to the inner container is achieved.
        ///// </summary>
        ///// <param name="context">Required.  The <see cref="ResolveContext"/>.</param>
        ///// <returns></returns>
        //protected sealed override ICompiledTarget GetFallbackCompiledTarget(ResolveContext context)
        //{
        //    return Inner.GetCompiledTarget(context.ChangeContainer(Inner));
        //}
    }
}
