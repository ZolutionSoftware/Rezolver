using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    /// <summary>
    /// Controls whether Rezolver's built-in array injection (which is also dependent upon the
    /// built-in IEnumerable injection) is enabled.
    /// 
    /// If not defined the <see cref="Default"/> is equivalent to <c>true</c>.
    /// </summary>
    public class EnableArrayInjection : ContainerOption<bool>
    {
        /// <summary>
        /// The default value for this option - equivalent to <c>true</c>
        /// </summary>
        public static EnableArrayInjection Default { get; } = true;

        /// <summary>
        /// Convenience conversion operator to <see cref="EnableArrayInjection"/> from <see cref="bool"/>
        /// </summary>
        /// <param name="value">The value to be wrapped as an <see cref="EnableArrayInjection"/> option value</param> 
        public static implicit operator EnableArrayInjection(bool value)
        {
            return new EnableArrayInjection() { Value = value };
        }
    }
}
