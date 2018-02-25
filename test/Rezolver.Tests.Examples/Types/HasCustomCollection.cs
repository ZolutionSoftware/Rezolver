using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class HasCustomCollection<T>
    {
        public CustomCollection<T> List { get; } = new CustomCollection<T>() { default(T) };
    }
    // </example>

    // <example2>
    public class HasWritableCustomCollection<T>
    {
        public CustomCollection<T> List { get; set; } = new CustomCollection<T>() { default(T) };
    }
    // </example2>
}
