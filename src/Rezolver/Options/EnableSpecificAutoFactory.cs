using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Options
{
    /// <summary>
    /// Internal-only option used to 'switch on' auto factory injection for a specific Func specialisation.
    /// </summary>
    internal class EnableSpecificAutoFactory : ContainerOption<bool>
    {
        public static EnableSpecificAutoFactory Default { get; } = false;

        public static implicit operator EnableSpecificAutoFactory(bool value)
        {
            return new EnableSpecificAutoFactory() { Value = value };
        }
    }
}
