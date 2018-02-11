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
}
