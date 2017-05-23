using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    public class PerServiceOptionGlobalFallback : ContainerOption<bool>
    {
        

        public static implicit operator PerServiceOptionGlobalFallback(bool value)
        {
            return new PerServiceOptionGlobalFallback() { Value = value };
        }
    }
}
