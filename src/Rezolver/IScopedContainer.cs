// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;

namespace Rezolver
{
  /// <summary>
  /// A container that's also a lifetime scope - that is, it's disposable,
  /// and will dispose of any disposable instances that it creates when it's disposed.
  /// 
  /// Also, any subsequent lifetime scopes that it, or any child, creates will 
  /// be disposed of when this scope is disposed.
  /// 
  /// Note that while a lifetime scope can track objects of any types, it only *automatically*
  /// tracks disposable objects.  To force a scope to track an instance, regardless of whether it's
  /// dispoable or not, you can call <see cref="AddToScope"/>.
  /// 
  /// This is how the default ScopedSingletonTarget works - if an object with this lifetime isn't a 
  /// disposable, it is explicitly added to the scope passed to it at runtime, and then when an instance
  /// is subsequently requested, the code compiled by the scoped singleton will search the current scope,
  /// for an existing instance, before creating one.
  /// </summary>
  public interface IScopedContainer : IContainer, IDisposable
  {
    /// <summary>
    /// If this lifetime scope is a child of another, this will be non-null.
    /// </summary>
    IScopedContainer ParentScope { get; }
    /// <summary>
    /// Registers an instance to this scope which, if disposable, will then be disposed 
    /// when this scope is disposed.
    /// </summary>
    /// <param name="obj">The object; if null, then no operation is performed.  Doesn't have to be IDisposable, but if it is, 
    /// then it will be tracked for disposal.</param>
    /// <param name="context">Optional - a rezolve context representing the conditions under which 
    /// the object should be returned in the enumerable returned from a call to GetFromScope</param>
    void AddToScope(object obj, RezolveContext context = null);

    /// <summary>
    /// Retrieves all objects from this scope that were previously added through a call to 
    /// <see cref="AddToScope" /> with RezolveContexts that match the one passed.
    /// 
    /// The method never returns null.
    /// </summary>
    /// <param name="context">Required - the context whose properties will be used to find matching
    /// objects.</param>
    /// <returns></returns>
    IEnumerable<object> GetFromScope(RezolveContext context);

    /// <summary>
    /// This event is fired before the scope disposes
    /// 
    /// This is primarily for infrastructure purposes, and not intended to be used from your code.
    /// </summary>
    event EventHandler Disposed;
  }
}