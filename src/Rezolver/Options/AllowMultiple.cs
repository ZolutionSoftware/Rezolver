using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Options
{
    /// <summary>
    /// Controls whether an <see cref="ITargetContainer"/> accepts multiple registered
    /// targets for the same underlying type.
    /// </summary>
    public class AllowMultiple : ContainerOption<bool>
    {
        /// <summary>
        /// Implicit conversion operator to this option type from <see cref="bool"/>,
        /// which simplifies the setting of this option.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator AllowMultiple(bool value)
        {
            return new AllowMultiple() { Value = value };
        }
    }
}
