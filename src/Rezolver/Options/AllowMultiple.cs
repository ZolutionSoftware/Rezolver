// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver.Options
{
    /// <summary>
    /// A <see cref="bool"/> option that controls whether an <see cref="ITargetContainer"/>
    /// accepts multiple registered targets for the same underlying type.
    ///
    /// This option can be applied globally or on a per-service basis - but must be set *before*
    /// any potentially affected registrations are performed.
    ///
    /// The <see cref="Default"/> (unset) is equivalent to <c>true</c>
    /// </summary>
    public class AllowMultiple : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for this option if not configured.
        ///
        /// Equivalent to <c>true</c>.
        /// </summary>
        public static AllowMultiple Default { get; } = true;
        /// <summary>
        /// Implicit conversion operator to this option type from <see cref="bool"/>,
        /// which simplifies setting this option with a simple <c>true</c> or <c>false</c>
        /// value.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator AllowMultiple(bool value)
        {
            return new AllowMultiple() { Value = value };
        }
    }
}
