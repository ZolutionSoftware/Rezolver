// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
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
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
        IResolveContext Previous { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.RequestedType"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.RequestedType", true)]
        Type RequestedType { get; }

        /// <summary>
        /// Obsolete"/>
        /// </summary>
        [Obsolete("Obsolete", true)]
        IContainer Container { get; }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
        IContainerScope Scope { get; }

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Resolve(Type)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.Resolve(Type)", true)]
        object Resolve(Type newRequestedType);

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.Resolve{TService}()"/>
        /// </summary>
        [Obsolete("Obsolete", true)]
        TResult Resolve<TResult>();

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
        bool TryResolve(Type newRequestedType, out object result);

        /// <summary>
        /// Obsolete"/>
        /// </summary>
        [Obsolete("Obsolete", true)]
        bool TryResolve<TResult>(out TResult result);

        /// <summary>
        /// Obsolete, use <see cref="ResolveContext.ChangeRequestedType(Type)"/> or <see cref="ResolveContext.ChangeContainer(Rezolver.Container)"/>
        /// </summary>
        [Obsolete("Obsolete - use ResolveContext.ChangeRequestedType or ChangeContainer", true)]
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
        /// Obsolete, use <see cref="Container.CanResolve(Type)"/>
        /// </summary>
        [Obsolete("Obsolete, use Container.CanResolve(Type)", true)]
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
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
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
        [Obsolete("Obsolete, use ContainerScope.Parent", true)]
        IContainerScope Parent { get; }

        /// <summary>
        /// Obsolete, use <see cref="ContainerScope2.Container"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Container", true)]
        Container Container { get; }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
        void ChildScopeDisposed(IContainerScope child);

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("Obsolete", true)]
        object Resolve(ResolveContext context, int targetId, Func<ResolveContext, object> factory, ScopeBehaviour behaviour);
    }

    /// <summary>
    /// Represents an object from which a scope can be created
    /// </summary>
    [Obsolete("This interface is now obsolete - Container, ContainerScope and ResolveContext now have dedicated CreateScope() functions", true)]
    public interface IScopeFactory
    {
        /// <summary>
        /// Creates a new scope.  If the implementing object is also a scope, then the new scope must be
        /// created as a child scope of that scope.
        /// </summary>
        [Obsolete("This interface is now obsolete - Container, ContainerScope and ResolveContext now have dedicated CreateScope() functions", true)]
        IContainerScope CreateScope();
    }
    /// <summary>
    /// Obsolete - container scope is available directly from <see cref="ContainerScope2.Root"/>
    /// </summary>
    [Obsolete("Root scope is now available directly from ContainerScope.Root", true)]
    public static class ContainerScopeExtensions
    {
        /// <summary>
        /// Obsolete - container scope is available directly from <see cref="ContainerScope2.Root"/>
        /// </summary>
        [Obsolete("Root scope is now available directly from ContainerScope.Root", true)]
        public static IContainerScope GetRootScope(this IContainerScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            while (scope.Parent != null) { scope = scope.Parent; }
            return scope;
        }
    }

    /// <summary>
    /// The extension methods in this class are no longer required.  Use ContainerScope directly
    /// </summary>
    [Obsolete("ContainerScopes now have a full suite of Resolve operations implemented as direct methods", true)]
    public static class ContainerScopeResolveExtensions
    {
        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope2"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static TResult Resolve<TResult>(this IContainerScope scope)
        {
            throw new NotSupportedException("This extension method is no longer available.");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope2"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static object Resolve(this IContainerScope scope, Type requestedType)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope2"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static IEnumerable ResolveMany(this IContainerScope scope, Type type)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope2"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static IEnumerable<TObject> ResolveMany<TObject>(this IContainerScope scope)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }
    }

}
