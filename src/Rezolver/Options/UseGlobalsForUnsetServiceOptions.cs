using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    /// <summary>
    /// A <see cref="bool"/> container option which controls whether per-service options will fallback to 
    /// a global option if not explicitly set.  The <see cref="Default"/> (unconfigured) is <c>true</c>.
    /// </summary>
    public class UseGlobalsForUnsetServiceOptions : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for this option if not configured.
        /// 
        /// Equivalent to <c>true</c>.
        /// </summary>
        public static UseGlobalsForUnsetServiceOptions Default { get; } = true;

        /// <summary>
        /// Implicit casting operator from <see cref="bool"/> which simplifies setting this option with
        /// a simple <c>true</c> or <c>false</c>.
        /// </summary>
        /// <param name="value">The boolean value to be stored in the returned instance.</param>
        public static implicit operator UseGlobalsForUnsetServiceOptions(bool value)
        {
            return new UseGlobalsForUnsetServiceOptions() { Value = value };
        }
    }
}
