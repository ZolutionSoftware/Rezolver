// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Extension methods for <see cref="IContainer"/> which provide shortcuts for the <see cref="IContainer.Resolve(ResolveContext)"/>
    /// operation.
    /// </summary>
    public static partial class ContainerResolveExtensions
    {
        /// <summary>
        /// Shortcut for resolving an IEnumerable of objects of a given type.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type of objects you want to retrieve</param>
        /// <returns>An enumerable (possibly empty) containing the objects which were resolved.</returns>
        public static IEnumerable ResolveMany(this Container container, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return (IEnumerable)container.Resolve(typeof(IEnumerable<>).MakeGenericType(type));
        }

        /// <summary>
        /// Shortcut for resolving an IEnumerable of <typeparamref name="TObject"/>
        /// </summary>
        /// <typeparam name="TObject">The type of objects expected in the enumerable</typeparam>
        /// <param name="container">The container.</param>
        /// <returns>An enumerable (possibly empty) containing the objects which were resolved.</returns>
        public static IEnumerable<TObject> ResolveMany<TObject>(this Container container)
        {
            return container.Resolve<IEnumerable<TObject>>();
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
        /// <remarks>For more detail on the <paramref name="type"/> parameter, see <see cref="Resolve(Container, Type)"/>
        /// overloads</remarks>
        public static bool TryResolve(this Container container, Type type, out object result)
        {
            return container.TryResolve(new ResolveContext(container, type), out result);
        }

        /// <summary>
        /// The same as the generic Resolve method the same core parameter types, except this will not throw
        /// exceptions if the resolve operation fails - instead it returns a boolean indicating success or failure,
        /// returning the created object (if successful) in the <paramref name="result"/> parameter.
        /// </summary>
        /// <typeparam name="TObject">The type to be resolved.</typeparam>
        /// <param name="container">The container.</param>
        /// <param name="result">Receives the object that is resolved if the operation is successful.</param>
        /// <returns><c>true</c> if the object was resolved, <c>false</c> otherwise.</returns>
        public static bool TryResolve<TObject>(this Container container, out TObject result)
        {
            var success = container.TryResolve(typeof(TObject), out object oResult);
            if (success)
            {
                result = (TObject)oResult;
            }
            else
            {
                result = default;
            }

            return success;
        }

        /// <summary>
        /// Determines whether this instance can resolve the specified type - wrapper for <see cref="IContainer.CanResolve(ResolveContext)"/>
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="type">The type.</param>
        public static bool CanResolve(this Container container, Type type)
        {
            return container.CanResolve(new ResolveContext(container, type));
        }

        /// <summary>
        /// Determines whether this instance can resolve the specified container - wrapper for <see cref="IContainer.CanResolve(ResolveContext)"/>
        /// </summary>
        /// <typeparam name="TObject">The type to be checked..</typeparam>
        /// <param name="container">The container.</param>
        public static bool CanResolve<TObject>(this Container container)
        {
            return container.CanResolve(typeof(TObject));
        }
    }
}
