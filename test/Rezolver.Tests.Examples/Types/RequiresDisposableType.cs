using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class RequiresDisposableType
    {
        public DisposableType Disposable { get; }
        public RequiresDisposableType(DisposableType disposable)
        {
            Disposable = disposable;
        }
    }
    // </example>
}
