// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;

namespace Rezolver
{

    /// <summary>
    /// A child container which extends another.
    /// 
    /// Note that this container gets its own singletons, scope, etc - everything is new; it's just the registrations
    /// which are shared.
    /// </summary>
    public sealed class OverridingContainer : Container
    {
        /// <summary>
        /// Gets the <see cref="Container"/> that is overriden by this container.
        /// </summary>
        public Container Inner { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="OverridingContainer"/>
        /// </summary>
        /// <param name="inner">Required.  The inner container that this one combines with.  Any dependencies not served
        /// by the new combined container's own targets will be sought from this container.  Equally, any targets in the base which
        /// are resolved when the overriding container is the root container for a resolve operation, will resolve
        /// their dependencies from this container.</param>
        /// <param name="config">Can be null.  A configuration to apply to this container (and, potentially its
        /// <see cref="Targets"/>).  If not provided, then the <see cref="Container.DefaultConfig"/> will be used</param>
        public OverridingContainer(Container inner, IContainerConfig config = null)
            : base(new OverridingTargetContainer(inner))
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));

            (config ?? DefaultConfig).Configure(this, Targets);
        }
    }
}
