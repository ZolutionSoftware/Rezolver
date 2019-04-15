// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections;
using System.Collections.Generic;

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
        /// Obsolete, use <see cref="Container.GetFactory(ResolveContext)"/>
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
    /// Obsolete in v2 use <see cref="ContainerScope"/>
    /// </summary>
    [Obsolete("This interface is now obsolete, use ContainerScope directly", true)]
    public interface IContainerScope : IDisposable, IServiceProvider, IScopeFactory
    {
        /// <summary>
        /// Obsolete, use <see cref="ContainerScope.Parent"/>
        /// </summary>
        [Obsolete("Obsolete, use ContainerScope.Parent", true)]
        IContainerScope Parent { get; }

        /// <summary>
        /// Obsolete, use <see cref="ContainerScope.Container"/>
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
    /// Obsolete - container scope is available directly from <see cref="ContainerScope.Root"/>
    /// </summary>
    [Obsolete("Root scope is now available directly from ContainerScope.Root", true)]
    public static class ContainerScopeExtensions
    {
        /// <summary>
        /// Obsolete - container scope is available directly from <see cref="ContainerScope.Root"/>
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
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static TResult Resolve<TResult>(this IContainerScope scope)
        {
            throw new NotSupportedException("This extension method is no longer available.");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static object Resolve(this IContainerScope scope, Type requestedType)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static IEnumerable ResolveMany(this IContainerScope scope, Type type)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }

        /// <summary>
        /// Obsolete - use the direct resolve operations declared directly on the <see cref="ContainerScope"/> class
        /// </summary>
        [Obsolete("Use the direct resolve operations declared directly on the ContainerScope class", true)]
        public static IEnumerable<TObject> ResolveMany<TObject>(this IContainerScope scope)
        {
            throw new NotSupportedException("This extension method is no longer available");
        }
    }

    /// <summary>
    /// Provides an abstraction for creating objects based on a given <see cref="ResolveContext"/> - this is
    /// the ultimate target of all <see cref="Container.Resolve(ResolveContext)"/> calls in the standard
    /// container implementations within the Rezolver framework.
    /// </summary>
    /// <remarks>An <see cref="Compilation.ITargetCompiler"/> creates instances of this from <see cref="ITarget"/>s which are
    /// registered in an <see cref="ITargetContainer"/>.
    ///
    /// When the container is then called upon to resolve an instance of a particular type, the <see cref="ICompiledTarget"/> is first
    /// obtained, and then the responsibility for creating the object is delegated to its <see cref="GetObject(ResolveContext)"/>
    /// method.</remarks>
    [Obsolete("No longer using ICompiledTarget - using Func<ResolveContext, object>, Func<ResolveContext, T> and IFactoryProvider/IFactoryProvider<T>", true)]
    public interface ICompiledTarget
    {
        /// <summary>
        /// Obsolete.
        /// </summary>
        [Obsolete("No longer using ICompiledTarget - using Func<ResolveContext, object>, Func<ResolveContext, T> and IFactoryProvider/IFactoryProvider<T>", true)]
        object GetObject(ResolveContext context);
        /// <summary>
        /// Gets the <see cref="ITarget"/> from which this compiled target was produced
        /// </summary>
        [Obsolete("No longer using ICompiledTarget - using Func<ResolveContext, object>, Func<ResolveContext, T> and IFactoryProvider/IFactoryProvider<T>", true)]
        ITarget SourceTarget { get; }
    }

    /// <summary>
    /// An <see cref="ICompiledTarget"/> that can be used when a type could not be resolved.
    ///
    /// Implementations of both <see cref="GetObject"/> and <see cref="SourceTarget"/> will throw an exception
    /// if called/read.
    /// </summary>
    /// <remarks>Use of this class is encouraged when an <see cref="IContainer"/> cannot resolve a type.  Instead of
    /// checking the compiled target for a null, an instance of this can be returned in its place, but its only when the
    /// <see cref="GetObject(ResolveContext)"/> method is executed that an exception occurs.
    ///
    /// This is particularly useful when using classes such as <see cref="OverridingContainer"/>, which allow dependencies
    /// that do not exist in the base container to be fulfilled by the overriding container instead: by delaying the throwing
    /// of exceptions until the resolve operation occurs, we are able to provide that override capability.</remarks>
    [Obsolete("No longer required", true)]
    public class UnresolvedTypeCompiledTarget : ICompiledTarget
    {
        private readonly Type _requestedType;

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.SourceTarget"/>
        /// </summary>
        /// <remarks>Always throws an <see cref="InvalidOperationException"/></remarks>
        public ITarget SourceTarget => throw new InvalidOperationException($"Could not resolve type {this._requestedType}");

        /// <summary>
        /// Creates a new instance of the <see cref="UnresolvedTypeCompiledTarget"/> class
        /// </summary>
        /// <param name="requestedType">Required.  The type that was requested, and which subsequently could not be resolved.</param>
        [Obsolete("No longer required", true)]
        public UnresolvedTypeCompiledTarget(Type requestedType)
        {
            this._requestedType = requestedType ?? throw new ArgumentNullException(nameof(requestedType));
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(ResolveContext)"/>
        /// </summary>
        /// <param name="context">The current <see cref="ResolveContext"/></param>
        /// <returns>Always throws an <see cref="InvalidOperationException"/></returns>
        public object GetObject(ResolveContext context) => throw new InvalidOperationException($"Could not resolve type {this._requestedType}");
    }

    /// <summary>
    /// An <see cref="ICompiledTarget"/> which wraps around an <see cref="IDirectTarget"/>.
    ///
    /// The implementation of <see cref="GetObject(ResolveContext)"/> simply executes the target's
    /// <see cref="IDirectTarget.GetValue"/> method.
    /// </summary>
    [Obsolete("No longer required", true)]
    internal class DirectCompiledTarget : ICompiledTarget
    {
        private readonly IDirectTarget _directTarget;

        public ITarget SourceTarget => this._directTarget;

        [Obsolete("No longer required", true)]
        public DirectCompiledTarget(IDirectTarget target)
        {
            this._directTarget = target;
        }

        public object GetObject(ResolveContext context)
        {
            return this._directTarget.GetValue();
        }
    }

    /// <summary>
    /// Obsolete.  See <see cref="IFactoryProvider"/> and <see cref="IFactoryProvider{T}"/>, and object factories
    /// are now just <see cref="Func{T, TResult}"/> with <see cref="ResolveContext"/> as the parameter type
    /// and either <see cref="Object"/> as the return type or a strongly-typed return type equal to the
    /// instance type.
    /// </summary>
    [Obsolete("No longer required - IFactoryProvider and IFactoryProvider<T> should now be used.", true)]
    public class DelegatingCompiledTarget : ICompiledTarget
    {
        private readonly Func<ResolveContext, object> _callback;

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.SourceTarget"/>
        /// </summary>
        public ITarget SourceTarget { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="DelegatingCompiledTarget"/> class.
        /// </summary>
        /// <param name="callback">Required.  The delegate to be executed when
        /// <see cref="GetObject(ResolveContext)"/> is called.</param>
        /// <param name="sourceTarget">Required.  The <see cref="ITarget"/> from which this
        /// <see cref="DelegatingCompiledTarget"/> is constructed.</param>
        public DelegatingCompiledTarget(Func<ResolveContext, object> callback, ITarget sourceTarget)
        {
            this._callback = callback ?? throw new ArgumentNullException(nameof(callback));
            SourceTarget = sourceTarget ?? throw new ArgumentNullException(nameof(sourceTarget));
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(ResolveContext)" /> - simply
        /// executes the delegate passed on construction.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object GetObject(ResolveContext context)
        {
            return this._callback(context);
        }
    }

    /// <summary>
    /// Obsolete, use <see cref="Func{T, TResult}"/>
    /// </summary>
    [Obsolete("Replace with Func<ResolveContext, object> or Func<ResolveContext, T>", true)]
    public class ConstantCompiledTarget : ICompiledTarget
    {
        private readonly object _obj;

        /// <summary>
        /// The target for which this compiled target was created.
        /// </summary>
        public ITarget SourceTarget { get; }
        /// <summary>
        /// Constructs a new instance of the <see cref="ConstantCompiledTarget"/>
        /// </summary>
        /// <param name="obj">The constant object to be returned by <see cref="GetObject(ResolveContext)"/></param>
        /// <param name="sourceTarget">The <see cref="ITarget"/> from which this compiled target is created.</param>
        public ConstantCompiledTarget(object obj, ITarget sourceTarget)
        {
            SourceTarget = sourceTarget ?? throw new ArgumentNullException(nameof(sourceTarget));
            this._obj = obj;
        }

        /// <summary>
        /// Implementation of <see cref="ICompiledTarget.GetObject(ResolveContext)"/> - simply returns the
        /// target with which this instance was constructed.
        /// </summary>
        /// <param name="context">ignored</param>
        /// <returns></returns>
        public object GetObject(ResolveContext context)
        {
            return this._obj;
        }
    }
}
