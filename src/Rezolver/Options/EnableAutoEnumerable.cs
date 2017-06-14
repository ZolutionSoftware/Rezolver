using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    public class EnableAutoEnumerable : ContainerOption<bool>
    {
        public static EnableAutoEnumerable Default { get; } = true;

        public static implicit operator EnableAutoEnumerable(bool value)
        {
            return new EnableAutoEnumerable() { Value = value };
        }
    }
}
