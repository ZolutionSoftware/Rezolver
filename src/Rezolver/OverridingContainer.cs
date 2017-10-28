// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Compilation;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainer"/> which can override the resolve operations of another.  This is useful when you have a 
    /// core application-wide container, with some objects being customised based on some ambient information,
    /// e.g. configuration, MVC Area/Controller, Brand (in a multi-tenant application for example) or more.
    /// </summary>
    /// <remarks>When overriding another <see cref="IContainer"/>, you are overriding the <see cref="ICompiledTarget"/> objects that
    /// will be returned when <see cref="IContainer.GetCompiledTarget(IResolveContext)"/> is called on that container and, therefore,
    /// the compiled target which is executed when the <see cref="IContainer.Resolve(IResolveContext)"/> method is called.
    /// 
    /// This has the side effect of overriding automatically resolved arguments (bound to a <see cref="ResolvedTarget"/>) compiled 
    /// in the overridden container by virtue of the fact that the overriding container is a different reference, because the
    /// <see cref="ResolvedTarget"/> is typically compiled with a check, at resolve-time, that the 
    /// <see cref="IResolveContext.Container"/> is the same container as the one that was active when it was originally compiled.
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
        /// <summary>
        /// Gets the <see cref="IContainer"/> that is overriden by this container.
        /// </summary>
        public IContainer Inner { get; }

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
        public OverridingContainer(IContainer inner, IRootTargetContainer targets = null, IContainerConfig config = null)
            : base(targets)
        {
            inner.MustNotBeNull("inner");
            Inner = inner;

            (config ?? DefaultConfig).Configure(this, Targets);
        }
        
        /// <summary>
        /// Called to determine if this container is able to resolve the type specified in the passed <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Required.  The <see cref="IResolveContext"/>.</param>
        /// <returns><c>true</c> if either this container or the inner container can resolve the 
        /// <see cref="IResolveContext.RequestedType"/>; otherwise <c>false</c></returns>
        public override bool CanResolve(IResolveContext context)
        {
            return base.CanResolve(context) || Inner.CanResolve(context);
        }

        /// <summary>
        /// Overrides the base implementation to pass the lookup for an <see cref="ITarget"/> to the inner container - this
        /// is how dependency chaining from this container to the inner container is achieved.
        /// </summary>
        /// <param name="context">Required.  The <see cref="IResolveContext"/>.</param>
        /// <returns></returns>
        protected override ICompiledTarget GetFallbackCompiledTarget(IResolveContext context)
        {
            return Inner.GetCompiledTarget(context);
        }
    }
}
