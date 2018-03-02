// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver
{
    /// <summary>
    /// An interface used by <see cref="TargetContainer"/> (via the options API)
    /// to identify the container type for targets with a specific <see cref="ITarget.DeclaredType"/>.
    /// </summary>
    public interface ITargetContainerTypeResolver
    {
        /// <summary>
        /// For operations such as <see cref="TargetDictionaryContainer.Fetch(Type)"/> and
        /// <see cref="TargetDictionaryContainer.FetchContainer(Type)"/>, the type requested might sometimes
        /// need to be redirected to another for the purposes of fetching the correct <see cref="ITargetContainer"/>
        /// for a given type of service.  If this method returns a non-null type, then the calling container
        /// will use that type instead of the original one passed when trying to locate a container for the
        /// service type.
        /// </summary>
        /// <param name="serviceType">The service type (equal to the <see cref="ITarget.DeclaredType"/> of
        /// any <see cref="ITarget"/> objects that might have been registered).</param>
        /// <returns>A type that should be used to look up a container, if different from the <paramref name="serviceType"/>,
        /// otherwise <c>null</c>.</returns>
        Type GetContainerType(Type serviceType);
    }
}
