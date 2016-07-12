﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
  public static partial class IContainerRezolveExtensions
  {
    /// <summary>
    /// Resolves an object of the given <paramref name="type"/>
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="type">The type to be resolved.</param>
    /// <returns>An instance of the <paramref name="type"/>.</returns>
    public static object Resolve(this IContainer container, Type type)
    {
      return container.Resolve(new RezolveContext(container, type));
    }

    /// <summary>
    /// Resolves an object of the given <paramref name="type" />, using the
    /// given lifetime <paramref name="scope" /> for lifetime management.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="type">Type to be resolved.</param>
    /// <param name="scope">The lifetime scope - can be a different instance to <paramref name="container"/>
    /// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
    /// master container instance is used to create a child 'scope' which has a different lifetime to that
    /// of the master.</param>
    /// <returns>An instance of the requested <paramref name="type"/>.</returns>
    public static object Resolve(this IContainer container, Type type, IScopedContainer scope)
    {
      return container.Resolve(new RezolveContext(container, type, scope));
    }

    /// <summary>
    /// Resolves an object of type <typeparamref name="TObject"/> 
    /// </summary>
    /// <typeparam name="TObject">The type to be resolved.</typeparam>
    /// <param name="container">The container.</param>
    /// <returns>An instance of <typeparamref name="TObject"/>.</returns>
    public static TObject Resolve<TObject>(this IContainer container)
    {
      return (TObject)container.Resolve(typeof(TObject));
    }

    /// <summary>
    /// Resolves an object of the type <typeparamref name="TObject" />, using the
    /// given lifetime <paramref name="scope" /> for lifetime management.
    /// </summary>
    /// <typeparam name="TObject">Type to be resolved.</typeparam>
    /// <param name="container">The container.</param>
    /// <param name="scope">The lifetime scope - can be a different instance to <paramref name="container"/>
    /// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
    /// master container instance is used to create a child 'scope' which has a different lifetime to that
    /// of the master.</param>
    /// <returns>An instance of <typeparamref name="TObject"/>.</returns>
    public static TObject Resolve<TObject>(this IContainer container, IScopedContainer scope)
    {
      return (TObject)container.Resolve(typeof(TObject), scope);
    }

    /// <summary>
    /// The same as the Resolve method with the same core parameter types, except this will not throw
    /// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
    /// returning the created object (if successful) in the <paramref name="result"/> parameter.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="type">The type to be resolved.</param>
    /// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
    /// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
    /// <remarks>For more detail on the <paramref name="type"/> parameter, see <see cref="Resolve(IContainer, Type)"/> 
    /// overloads</remarks>
    public static bool TryResolve(this IContainer container, Type type, out object result)
    {
      return container.TryResolve(new RezolveContext(container, type), out result);
    }

    /// <summary>
    /// The same as the Resolve method with the same core parameter types, except this will not throw
    /// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure, 
    /// returning the created object (if successful) in the <paramref name="result"/> parameter.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="type">The type to be resolved.</param>
    /// <param name="scope">The lifetime scope - can be a different instance to <paramref name="container"/>
    /// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
    /// master container instance is used to create a child 'scope' which has a different lifetime to that
    /// of the master.</param>
    /// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
    /// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
    /// <remarks>For more detail on the <paramref name="type"/> parameter, see <see cref="Resolve(IContainer, Type, IScopedContainer)"/> 
    /// overloads</remarks>
    public static bool TryResolve(this IContainer container, Type type, IScopedContainer scope, out object result)
    {
      return container.TryResolve(new RezolveContext(container, type, scope), out result);
    }

    /// <summary>
    /// The same as the generic Resolve method the same core parameter types, except this will not throw
    /// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
    /// returning the created object (if successful) in the <paramref name="result"/> parameter.
    /// </summary>
    /// <typeparam name="TObject">The type to be resolved.</typeparam>
    /// <param name="container">The container.</param>
    /// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
    /// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
    public static bool TryResolve<TObject>(this IContainer container, out TObject result)
    {
      object oResult;
      var success = container.TryResolve(out oResult);
      if (success)
        result = (TObject)oResult;
      else
        result = default(TObject);
      return success;
    }

    /// <summary>
    /// The same as the generic Resolve method the same core parameter types, except this will not throw
    /// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
    /// returning the created object (if successful) in the <paramref name="result"/> parameter.
    /// </summary>
    /// <typeparam name="TObject">The type to be resolved.</typeparam>
    /// <param name="container">The container.</param>
    /// <param name="scope">The lifetime scope - can be a different instance to <paramref name="container"/>
    /// (or the object on which the method is invoked if invoked as an extension method), e.g. when a 
    /// master container instance is used to create a child 'scope' which has a different lifetime to that
    /// of the master.</param>
    /// <param name="result">Received the value, or a reference to the instance, that is resolved if the operation is successful.</param>
    /// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
    public static bool TryResolve<TObject>(this IContainer container, IScopedContainer scope, out TObject result)
    {
      object oResult;
      var success = container.TryResolve(scope, out oResult);
      if (success)
        result = (TObject)oResult;
      else
        result = default(TObject);
      return success;
    }
  }
}
