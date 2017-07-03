using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Options
{
    public class LazyLoadedEnumerables : ContainerOption<bool>
    {
        public static LazyLoadedEnumerables Default { get; } = true;

        public static implicit operator LazyLoadedEnumerables(bool value)
        {
            return new LazyLoadedEnumerables() { Value = value };
        }
    }
}
