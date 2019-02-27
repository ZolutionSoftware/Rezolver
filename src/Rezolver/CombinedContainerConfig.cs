// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Sdk;

namespace Rezolver
{
    /// <summary>
    /// An <see cref="IContainerConfig"/> which contains zero or or more other <see cref="IContainerConfig"/>
    /// objects.  Behaviours can depend on other behaviours, and this collection ensures that they are applied
    /// in the correct order.
    /// </summary>
    /// <seealso cref="CombinedTargetContainerConfig"/>
    public class CombinedContainerConfig : DependantCollection<IContainerConfig>, IContainerConfig
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedContainerConfig"/> type
        /// </summary>
        public CombinedContainerConfig()
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedContainerConfig"/> class by cloning an existing one.
        /// </summary>
        /// <param name="collection">The collection whose elements are to be copied.  If null, then the collection is initialised
        /// empty.</param>
        public CombinedContainerConfig(CombinedContainerConfig collection)
            : this((IEnumerable<IContainerConfig>)collection)
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedContainerConfig"/> type, using the passed behaviours
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public CombinedContainerConfig(IEnumerable<IContainerConfig> behaviours)
            : base(behaviours)
        {
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="CombinedContainerConfig"/> type, using the passed behaviours
        /// enumerable to seed the underlying collection.
        /// </summary>
        /// <param name="behaviours">The behaviours to be added to the collection on construction.</param>
        public CombinedContainerConfig(params IContainerConfig[] behaviours)
            : this((IEnumerable<IContainerConfig>)behaviours)
        {
        }

        /// <summary>
        /// Clones the configuration in this instance into a new instance of the <see cref="CombinedContainerConfig"/>
        /// type.
        /// </summary>
        public CombinedContainerConfig Clone() => base.Clone<CombinedContainerConfig>();

        /// <summary>
        /// Applies the behaviours in this collection to the passed <paramref name="container"/> and
        /// <paramref name="targets"/>.
        /// </summary>
        /// <param name="container">The container to which the behaviours are being attached.</param>
        /// <param name="targets">The target container used by the <paramref name="container"/> for its registrations.</param>
        /// <remarks>The implementation runs through each behaviour that has been added to the collection, in dependency
        /// order, calling its <see cref="IContainerConfig.Configure(Container, IRootTargetContainer)"/> method.</remarks>
        public void Configure(Container container, IRootTargetContainer targets)
        {
            foreach (var behaviour in Ordered)
            {
                behaviour.Configure(container, targets);
            }
        }
    }
}
