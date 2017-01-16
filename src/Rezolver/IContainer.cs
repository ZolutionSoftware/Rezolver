// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
  /// <summary>
  /// The primary IOC container interface in the Rezolver framework.
  /// </summary>
  /// <remarks>Instances of objects (typically known as 'services' in IOC-speak) are resolved via the <see cref="Resolve"/> or <see cref="TryResolve(ResolveContext, out object)"/> methods.
  /// 
  /// You can introspect the container to check in advance whether a given type can be resolved through the <see cref="CanResolve(ResolveContext)"/> method.
  /// 
  /// Lifetime scoping (where <see cref="IDisposable"/> objects are created within a scope and destroyed when that scope is destroyed) can be achieved
  /// by obtaining a new scope through the <see cref="CreateLifetimeScope"/> method.
  /// 
  /// Many of the functions which accept a <see cref="ResolveContext"/> also have alternatives (in the form of extension methods) in the 
  /// <see cref="IContainerRezolveExtensions"/> class.  If you're looking for the 'traditional' IOC container methods (e.g. <see cref="IContainerRezolveExtensions.Resolve{TObject}(IContainer)"/>,
  /// that's where you'll find them.
  /// 
  /// Note that while the standard implementation of this interface supplied by the framework (<see cref="ContainerBase" /> and its derivatives) all utilitise
  /// the <see cref="ITargetContainer"/> to actually locate registrations for types which are ultimately requested from the container; the interface
  /// doesn't actually mandate that pattern.
  /// </remarks>
  public interface IContainer : IServiceProvider
  {
    /// <summary>
    /// Returns true if a resolve operation for the given context will succeed.
    /// If you're going to be calling <see cref="Resolve(ResolveContext)"/> immediately afterwards, consider using the 
    /// <see cref="TryResolve(ResolveContext, out object)"/> method instead, which allows you to check and obtain the result at the same time.
    /// </summary>
    /// <param name="context">The resolve context.</param>
    /// <returns><c>true</c> if this instance can resolve a type for the specified context; otherwise, <c>false</c>.</returns>
    bool CanResolve(ResolveContext context);

    /// <summary>
    /// Called to resolve a reference to an object for the given context (which provides the <see cref="ResolveContext.RequestedType"/> of the 
    /// object that is required, among other things).
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The resolved object, if successful.</returns>
    /// <exception cref="System.InvalidOperationException">If the requested type cannot be resolved.</exception>
    object Resolve(ResolveContext context);

    /// <summary>
    /// Merges the <see cref="CanResolve(ResolveContext)"/> and <see cref="Resolve(ResolveContext)"/> operations into one operation.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="result">Receives the resultant resolved object if the operation succeeds.</param>
    /// <returns><c>true</c> if the operation succeeded (the resolved object will be set into the <paramref name="result"/> 
    /// parameter); <c>false</c> otherwise.</returns>
    bool TryResolve(ResolveContext context, out object result);

    /// <summary>
    /// Called to create an <see cref="IScopedContainer" /> which can resolve the same objects as this container, but which will track,
    /// and dispose of, any disposable objects that it creates.
    /// </summary>
    /// <remarks>If the container on which this is called also implements the <see cref="IScopedContainer"/> interface, then the newly created
    /// scope's lifetime will be tied to that parent container.</remarks>
    IScopedContainer CreateLifetimeScope();

    /// <summary>
    /// Fetches the <see cref="ICompiledTarget"/> for the given context, whose <see cref="ICompiledTarget.GetObject(ResolveContext)"/> method 
    /// would ultimately be called if the same context was passed to <see cref="Resolve(ResolveContext)"/> or <see cref="TryResolve(ResolveContext, out object)"/>.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <remarks>This is not typically a method that you're likely to use as the consumer of an <see cref="IContainer" />.
    /// It's more typically used by implementations of <see cref="ITargetCompiler"/> or <em>implementations</em> of <see cref="IContainer"/> when communicating
    /// with other containers.
    /// 
    /// As such, its use at an application level is limited.
    /// </remarks>
    ICompiledTarget FetchCompiled(ResolveContext context);
  }
}