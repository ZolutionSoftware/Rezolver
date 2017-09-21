using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class DefaultComparerWrapper<T> : IComparer<T>
    {
        int IComparer<T>.Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }
    }
    // </example>
}
