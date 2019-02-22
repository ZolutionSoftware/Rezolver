// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver
{
    /// <summary>
    /// A target container option used by <see cref="TargetContainer"/> to create a new instance of
    /// an <see cref="ITargetContainer"/> to store target registrations for a specific type (or types
    /// relating to a specific type).
    /// </summary>
    public interface ITargetContainerFactory
    {
        /// <summary>
        /// Creates an <see cref="ITargetContainer"/> suitable for registration in the <paramref name="targets"/>
        /// parent container for targets whose <see cref="ITarget.DeclaredType"/> is equal, or otherwise related,
        /// to the passed <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A type that is, in some way, common to all targets or child target containers that will
        /// be registered in the required container.</param>
        /// <param name="targets">The target container into which the returned target container will be registered.</param>
        /// <returns>An <see cref="ITargetContainer"/> to be added to the <paramref name="targets"/> into which targets
        /// will be registered.  Or <c>null</c> if this factory doesn't handle the passed <paramref name="type"/>.</returns>
        ITargetContainer CreateContainer(Type type, ITargetContainer targets);
    }
}
