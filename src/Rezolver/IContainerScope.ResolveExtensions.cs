// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Extensions for <see cref="IContainerScope"/> so that instances of that
    /// interface present a similar set of functionality to <see cref="IContainer"/>.
    /// </summary>
    public static class ContainerScopeResolveExtensions
    {
        /// <summary>
        /// Resolves an object through the scope's <see cref="IContainerScope.Container"/>
        /// </summary>
        /// <typeparam name="TResult">The type of object required.</typeparam>
        /// <param name="scope">The active scope within which the call is to be made.</param>
        /// <returns></returns>
        /// <remarks>Resolving an object via a scope does not guarantee that it will be
        /// tracked.  Ultimately, it's up to the behaviour of the individual underlying
        /// targets to determine whether they should interact with the scope.
        ///
        /// Indeed, all this extension method does is to forward the method call on to the
        /// <see cref="IContainerScope.Container"/> of the given scope, ensuring that
        /// the scope is set on the <see cref="ResolveContext"/> that is passed to its
        /// <see cref="IContainer.Resolve(ResolveContext)"/> method.
        /// </remarks>
        public static TResult Resolve<TResult>(this IContainerScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return (TResult)scope.Container.Resolve(new ResolveContext(scope, typeof(TResult)));
        }

        /// <summary>
        /// Non-generic variant of the <see cref="Resolve{TResult}(IContainerScope)"/>
        /// extension method.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="requestedType">Type of object required.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static object Resolve(this IContainerScope scope, Type requestedType)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return scope.Container.Resolve(new ResolveContext(scope, requestedType));
        }

        /// <summary>
        /// Equivalent of <see cref="ContainerResolveExtensions.ResolveMany(IContainer, Type)"/>
        /// but for scopes.
        /// </summary>
        /// <param name="scope">The scope from which objects are to be resolved.</param>
        /// <param name="type">The type of object desired in the enumerable.</param>
        /// <returns>An enumerable (possibly empty) containing all the objects that could
        /// be resolve of type <paramref name="type"/></returns>
        public static IEnumerable ResolveMany(this IContainerScope scope, Type type)
        {
            return (IEnumerable)Resolve(
                scope,
                typeof(IEnumerable<>).MakeGenericType(
                    type ?? throw new ArgumentNullException(nameof(type))));
        }

        /// <summary>
        /// Equivalent of <see cref="ContainerResolveExtensions.ResolveMany{TObject}(IContainer)"/>
        /// but for scopes.
        /// </summary>
        /// <typeparam name="TObject">The type of object desired in the enumerable.</typeparam>
        /// <param name="scope">The scope from which objects are to be resolved.</param>
        /// <returns>An enumerable (possibly empty) containing all the objects that could be
        /// resolved of type <typeparamref name="TObject"/></returns>
        public static IEnumerable<TObject> ResolveMany<TObject>(this IContainerScope scope)
        {
            return Resolve<IEnumerable<TObject>>(scope);
        }
    }
}
