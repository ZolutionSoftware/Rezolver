using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Options
{
    /// <summary>
    /// Global-only option for controlling whether the <see cref="Configuration.InjectAutoFactories"/> configuration is enabled, 
    /// which, in turn, makes it possible to use the auto-factory registration extensions in <see cref="AutoFactoryRegistrationExtensions"/>.
    /// 
    /// The <see cref="Default"/> (i.e. unconfigured) value is equivalent to <c>true</c>.
    /// </summary>
    public class EnableAutoFactoryInjection : ContainerOption<bool>
    {
        /// <summary>
        /// Default value for this option, equivalent to <c>true</c>
        /// </summary>
        public static EnableAutoFactoryInjection Default { get; } = true;

        /// <summary>
        /// Implicit conversion operator from <see cref="Boolean"/>
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableAutoFactoryInjection(bool value)
        {
            return new EnableAutoFactoryInjection() { Value = value };
        }
    }
}
