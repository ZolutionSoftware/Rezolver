// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainer"/> that also has a <see cref="Scope"/> attached to it.
    ///
    /// Many applications will use such a container as the root container to use as anchors
    /// for things like scoped singletons.
    /// </summary>
    /// <seealso cref="Rezolver.IContainer" />
    /// <seealso cref="System.IDisposable" />
    public interface IScopedContainer : IContainer, IDisposable
    {
        /// <summary>
        /// Gets the root scope for this scoped container.
        ///
        /// Note that this is used automatically by the container for <see cref="IContainer.Resolve(ResolveContext)"/>
        /// operations where the <see cref="ResolveContext.Scope"/> property is not already set.
        /// </summary>
        IContainerScope Scope { get; }
    }
}