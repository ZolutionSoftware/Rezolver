// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    // Exclusively used to mark interfaces and types which are 

    /// <summary>
    /// This interface is now obsolete in v2.0
    /// </summary>
    [Obsolete("IResolveContext now obsolete from Rezolver v2.0 - use ResolveContext", true)]
    public interface IResolveContext
    {
        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Previous"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Previous", true)]
        IResolveContext Previous { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.RequestedType"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.RequestedType", true)]
        Type RequestedType { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Container"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Container", true)]
        IContainer Container { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Scope"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Scope", true)]
        IContainerScope Scope { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Resolve(Type)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Resolve(Type)", true)]
        object Resolve(Type newRequestedType);

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Resolve{TResult}"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Resolve{TResult}", true)]
        TResult Resolve<TResult>();

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.TryResolve(Type, out object)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.TryResolve(Type, out object)", true)]
        bool TryResolve(Type newRequestedType, out object result);

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.TryResolve{TResult}(out TResult)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.TryResolve{TResult}(out TResult)", true)]
        bool TryResolve<TResult>(out TResult result);

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.New(Type, Rezolver.Container, IContainerScope)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.New(Type, Container, IContainerScope)", true)]
        IResolveContext New(Type newRequestedType = null,
            IContainer newContainer = null,
            IContainerScope newScope = null);
    }

    /// <summary>
    /// Obsolete interface - now use the <see cref="Container"/> class
    /// </summary>
    [Obsolete("Use Container", true)]
    public interface IContainer : IScopeFactory, IServiceProvider
    {
        /// <summary>
        /// Obsolete, use <see cref="Container.CanResolve(ResolveContext)"/>
        /// </summary>
        [Obsolete("Obsolete, use Container.CanResolve(ResolveContext)", true)]
        bool CanResolve(ResolveContext context);

        /// <summary>
        /// Obsolete, use <see cref="Container.Resolve(ResolveContext)"/>
        /// </summary>
        [Obsolete("Obsolete, use Container.Resolve(ResolveContext)", true)]
        object Resolve(ResolveContext context);

        /// <summary>
        /// Obsolete, use <see cref="Container.TryResolve(ResolveContext, out object)"/>
        /// </summary>
        [Obsolete("Obsolete, use Container.TryResolve(ResolveContext, out object)", true)]
        bool TryResolve(ResolveContext context, out object result);

        /// <summary>
        /// Obsolete, use <see cref="Container.GetCompiledTarget(ResolveContext)"/>
        /// </summary>
        [Obsolete("Obsolete, use Container.GetCompiledTarget(ResolveContext)", true)]
        ICompiledTarget GetCompiledTarget(ResolveContext context);
    }

    /// <summary>
    /// Obsolete - use <see cref="ScopedContainer"/>
    /// </summary>
    [Obsolete("IScopedContainer is obsolete - use ScopedContainer", true)]
    public interface IScopedContainer : IContainer, IDisposable
    {
        /// <summary>
        /// Obsolete, use <see cref="ScopedContainer.Scope"/>
        /// </summary>
        [Obsolete("Obsolete, use ScopedContainer.Scope", true)]
        IContainerScope Scope { get; }
    }

    /// <summary>
    /// Obsolete in v2 use <see cref="ContainerScope2"/>
    /// </summary>
    [Obsolete("This interface is now obsolete, use ContainerScope directly", true)]
    public interface IContainerScope : IDisposable, IServiceProvider, IScopeFactory
    {
        /// <summary>
        /// Obsolete, use <see cref="ContainerScope2.Parent"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Parent")]
        IContainerScope Parent { get; }

        /// <summary>
        /// Obsolete, use <see cref="ContainerScope2.Container"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Container")]
        Container Container { get; }

        /// <summary>
        /// Obsolete, use <see cref="ContainerScope2"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Parent")]
        void ChildScopeDisposed(IContainerScope child);

        /// <summary>
        /// Obsolete, use <see cref="ContainerScope2.Parent"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Parent")]
        object Resolve(ResolveContext context, int targetId, Func<ResolveContext, object> factory, ScopeBehaviour behaviour);
    }

    /// <summary>
    /// Represents an object from which a scope can be created
    /// </summary>
    [Obsolete("This interface is now obsolete - Container, ContainerScope and ResolveContext now have dedicated CreateScope() functions")]
    public interface IScopeFactory
    {
        /// <summary>
        /// Creates a new scope.  If the implementing object is also a scope, then the new scope must be
        /// created as a child scope of that scope.
        /// </summary>
        [Obsolete("This interface is now obsolete - Container, ContainerScope and ResolveContext now have dedicated CreateScope() functions")]
        IContainerScope CreateScope();
    }
}
