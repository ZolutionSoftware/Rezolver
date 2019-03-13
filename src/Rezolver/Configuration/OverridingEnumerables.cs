// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Configuration
{
    /// <summary>
    /// When applied to an <see cref="OverridingContainer"/> whose target container has been configured to
    /// enable automatically injected enumerables (via the <see cref="InjectEnumerables"/> configuration callback and the
    /// <see cref="Options.EnableEnumerableInjection"/> option), then this will extend enumerable support in the
    /// <see cref="OverridingContainer"/> to construct enumerables made up of a combination of all services in the
    /// overriden container AND those from the overriding container.
    /// </summary>
    /// <remarks>
    /// Note that this class is not an <see cref="ITargetContainerConfig"/> like the <see cref="InjectEnumerables"/>,
    /// instead it is an <see cref="IContainerConfig"/> because it's only relevant for instances of <see cref="OverridingContainer"/>.
    ///
    /// As such, when applied to an <see cref="IContainer"/> instance, it will only auto-attach when the container is an instance
    /// of (or derived from) <see cref="OverridingContainer"/> *and* if the <see cref="Options.EnableEnumerableInjection"/> options evaluates
    /// to <c>true</c> when read from the <see cref="ITargetContainer"/> passed to <see cref="Configure(Container, IRootTargetContainer)"/>.</remarks>
    public sealed class OverridingEnumerables : IContainerConfig
    {
        /// <summary>
        /// The one and only instance of the <see cref="OverridingEnumerables"/>
        /// </summary>
        public static OverridingEnumerables Instance { get; } = new OverridingEnumerables();

        private OverridingEnumerables() { }

        /// <summary>
        /// If <paramref name="container"/> is an <see cref="OverridingContainer"/>, and if the <see cref="Options.EnableEnumerableInjection"/>
        /// option evaluates to <c>true</c> (the default) when read from <paramref name="targets"/>, then enumerable handling in the container
        /// will be extended to combine the enumerables from both objects registered specifically in the <paramref name="container"/>, plus also those
        /// registered in its <see cref="OverridingContainer.Inner"/> container.
        /// </summary>
        /// <param name="container">The container to be configured.</param>
        /// <param name="targets">The <see cref="ITargetContainer"/> which supplies the registrations for the <paramref name="container"/></param>
        public void Configure(Container container, IRootTargetContainer targets)
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            if (container is OverridingContainer && targets.GetOption(Options.EnableEnumerableInjection.Default))
            {
                targets.RegisterContainer(typeof(IEnumerable<>),
                    new ConcatenatingEnumerableContainer(container, targets));
            }
        }
    }
}
