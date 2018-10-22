// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rezolver
{
    /// <summary>
    /// A version of <see cref="TargetContainer" /> which overrides and extends the registrations of another
    /// (the <see cref="Parent" />).
    /// </summary>
    /// <seealso cref="Rezolver.TargetContainer" />
    /// <remarks>When this class searches for an entry for a type, if it
    /// cannot find one within its own registrations, it falls back to the registrations of
    /// its ancestors (starting with its <see cref="Parent" />).
    ///
    /// As a result, any dependencies required by registrations in this container can be provided by
    /// any ancestor.
    ///
    /// This fallback logic in the <see cref="Fetch(Type)" /> is triggered by the
    /// <see cref="ITarget.UseFallback" /> property.
    ///
    /// The <see cref="FetchAll(Type)"/> method, however, returns all targets registered directly in this
    /// container and in the parent.</remarks>
    public sealed class OverridingTargetContainer : TargetContainer
    {
        /// <summary>
        /// Gets the parent target container.
        /// </summary>
        /// <value>The parent.</value>
        public ITargetContainer Parent { get; private set; }

        /// <summary>
        /// Overrides the base implementation so that the root is derived from the <see cref="Parent"/>.
        /// </summary>
        public override IRootTargetContainer Root => Parent.Root;

        /// <summary>
        /// Initializes a new instance of the <see cref="OverridingTargetContainer"/> class.
        /// </summary>
        /// <param name="parent">Required. The parent target container.</param>
        /// <param name="config">Optional.  The configuration to apply to this target container.  If null, then
        /// the <see cref="TargetContainer.DefaultConfig"/> is used.
        /// </param>
        public OverridingTargetContainer(ITargetContainer parent, ITargetContainerConfig config = null)
                : base()
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            (config ?? DefaultConfig).Configure(this);
        }


        /// <summary>
        /// Fetches the registered target for the given <paramref name="type"/>, if found, or
        /// forwards the call to the <see cref="Parent"/> container.
        /// </summary>
        /// <param name="type">The type whose registration is sought.</param>
        /// <returns>The target which is registered for the given type, or null if no registration
        /// can be found.
        /// </returns>
        public override ITarget Fetch(Type type)
        {
            var result = base.Fetch(type);
            // ascend the tree of target containers looking for a type match.
            if ((result == null || result.UseFallback))
            {
                return Parent.Fetch(type);
            }

            return result;
        }

        /// <summary>
        /// Implementation of <see cref="ITargetContainer.FetchAll(Type)" /> which returns
        /// an enumerable of targets from both the base target container and this target container.
        /// </summary>
        /// <param name="type">The type whose targets are to be retrieved.</param>
        /// <returns>A non-null enumerable containing the targets that match the type, or an
        /// empty enumerable if the type is not registered.</returns>
        public override IEnumerable<ITarget> FetchAll(Type type)
        {
            return (Parent.FetchAll(type) ?? Enumerable.Empty<ITarget>()).Concat(
                base.FetchAll(type) ?? Enumerable.Empty<ITarget>());
        }
    }
}