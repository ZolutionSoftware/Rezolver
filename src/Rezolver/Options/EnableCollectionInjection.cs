// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    /// <summary>
    /// Boolean option which, if configured before the <see cref="Configuration.InjectCollections"/> configuration is applied,
    /// will control whether automatic injection of <see cref="System.Collections.ObjectModel.ReadOnlyCollection{T}"/> (and related interfaces - see the documentation
    /// on that type for more) will be enabled.
    /// The <see cref="Default"/> is equivalent to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// The injection behaviour controlled by this option most commonly piggybacks off of the behaviour that's enabled by the
    /// <see cref="Configuration.InjectEnumerables"/> configuration.
    ///
    /// However, although this option is therefore related to the <see cref="EnableEnumerableInjection"/> option, the two are independent.
    ///
    /// If you disable automatic enumerable injection, it does not automatically disable automatic list injection.</remarks>
    /// <seealso cref="Configuration.InjectLists"/>
    /// <seealso cref="EnableEnumerableInjection"/>
    /// <seealso cref="Configuration.InjectEnumerables"/>
    public class EnableCollectionInjection : ContainerOption<bool>
    {
        /// <summary>
        /// The Default setting for the <see cref="EnableCollectionInjection"/> option - evaluates to <c>true</c>
        /// </summary>
        public static EnableCollectionInjection Default { get; } = true;

        /// <summary>
        /// Convenience operator for treating booleans as <see cref="EnableCollectionInjection"/> option values.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableCollectionInjection(bool value)
        {
            return new EnableCollectionInjection() { Value = value };
        }
    }
}
