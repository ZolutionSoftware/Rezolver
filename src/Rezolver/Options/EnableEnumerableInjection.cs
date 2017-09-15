using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    /// <summary>
    /// Controls whether the <see cref="Container.DefaultConfig"/> will enable automatically injected enumerables
    /// or not.
    /// </summary>
    public class EnableEnumerableInjection : ContainerOption<bool>
    {

        /// <summary>
        /// The default value for this option - equivalent to <c>true</c>
        /// </summary>
        public static EnableEnumerableInjection Default { get; } = true;

        /// <summary>
        /// Convenience operator for creating an instance of this option from a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableEnumerableInjection(bool value)
        {
            return new EnableEnumerableInjection() { Value = value };
        }
    }
}
