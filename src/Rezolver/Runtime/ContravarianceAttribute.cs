using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Runtime
{
    /// <summary>
    /// Internal hack to prevent stack overflows when the <see cref="OptionsTargetContainerExtensions"/>
    /// options accessors recurse for other options lookups.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal class ContravarianceAttribute : Attribute
    {
        public bool Enable { get; }
        public ContravarianceAttribute(bool enable)
        {
            Enable = enable;
        }
    }
}
