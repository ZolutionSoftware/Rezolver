using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Options
{
    /// <summary>
    /// Option which controls whether the <see cref="Configuration.InjectAutoFuncs"/> configuration is applied.
    /// </summary>
    public class EnableAutoFuncInjection : ContainerOption<bool>
    {
        /// <summary>
        /// The Default setting for the <see cref="EnableAutoFuncInjection"/> option - evaluates to <c>true</c>
        /// </summary>
        public static EnableAutoFuncInjection Default { get; } = true;

        /// <summary>
        /// Convenience operator for treating booleans as <see cref="EnableAutoFuncInjection"/> option values.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator EnableAutoFuncInjection(bool value)
        {
            return new EnableAutoFuncInjection() { Value = value };
        }
    }
}
