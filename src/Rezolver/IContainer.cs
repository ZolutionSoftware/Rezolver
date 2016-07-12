// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
  /// <summary>
  /// Represents the primary IOC container in the Rezolver framework.
  /// </summary>
  /// <remarks>Instances of objects are resolved via the <see cref="Resolve"/> or <see cref="TryResolve(RezolveContext, out object)"/> methods.
  /// 
  /// You can introspect the container to check in advance whether a given type can be resolved through the <see cref="CanResolve(RezolveContext)"/> method.
  /// 
  /// Lifetime scoping (where <see cref="IDisposable"/> objects are created within a scope and destroyed when that scope is destroyed) can be achieved
  /// by obtaining a new scope through the <see cref="CreateLifetimeScope"/> method.
  /// 
  /// Many of the functions which accept a <see cref="RezolveContext"/> also have alternatives (in the form of extension methods) in the 
  /// <see cref="IContainerRezolveExtensions"/> class.
  /// </remarks>
  public interface IContainer : IServiceProvider
  {
    /// <summary>
    /// Returns true if a resolve operation for the given context will succeed.
    /// If you're going to be calling <see cref="Resolve"/> immediately afterwards, consider using the TryResolve method instead,
    /// which allows you to check and obtain the result at the same time.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns><c>true</c> if this instance can resolve the specified context; otherwise, <c>false</c>.</returns>
    bool CanResolve(RezolveContext context);

    /// <summary>
    /// The core 'resolve' operation for a Rezolver container.
    /// 
    /// The object is resolved using state from the passed <paramref name="context"/>, including any active lifetime scope and 
    /// a reference to the original container instance that was called, which could be a different container to this one.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>The resolved object, if successful.</returns>
    /// <exception cref="System.InvalidOperationException">If the requested type cannot be resolved.</exception>
    object Resolve(RezolveContext context);

    /// <summary>
    /// Merges the CanResolve and Resolve operations into one call.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="result">The result.</param>
    /// <returns><c>true</c> if the operation succeeded (the resolved object will be set into the <paramref name="result"/> 
    /// parameter); <c>false</c> otherwise.</returns>
    bool TryResolve(RezolveContext context, out object result);

    /// <summary>
    /// Called to create a lifetime scope with the same configuration as this container that will track, and dispose of, any 
    /// disposable objects that are created via calls to its <see cref="Resolve"/>
    /// </summary>
    /// <returns>An <see cref="IScopedContainer"/> instance that will use this resolver to resolve objects,
    /// but which will impose its own lifetime restrictions on those instances.</returns>
    IScopedContainer CreateLifetimeScope();

    /// <summary>
    /// Fetches the compiled target for the given context.
    /// 
    /// This is not typically a method that consumers of an <see cref="IContainer" /> are likely to use; it's more typically
    /// used by code generation code (or even generated code) to interoperate between two resolvers, or indeed over other object.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns>ICompiledRezolveTarget.</returns>
    ICompiledTarget FetchCompiled(RezolveContext context);
  }
}