// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver.Options
{
    /// <summary>
    /// Option that controls whether or not auto-injected enumerables - either globally, or for a
    /// given type - will perform covariant searches when locating targets to be included in the
    /// enumerable. The <see cref="Default"/> is equivalent to <c>true</c>.
    /// </summary>
    public class EnableEnumerableCovariance : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for this option - equivalent to <c>true</c>
        /// </summary>
        public static EnableEnumerableCovariance Default { get; } = true;

        /// <summary>
        /// Convenience convserion operator from <see cref="bool"/> to <see cref="EnableEnumerableCovariance"/>
        /// </summary>
        /// <param name="value">The boolean value to wrapped in a new instance of <see cref="EnableEnumerableCovariance"/></param>
        public static implicit operator EnableEnumerableCovariance(bool value)
        {
            return new EnableEnumerableCovariance() { Value = value };
        }
    }
}
