using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresDisposable : IDisposable
    {
        public int DisposedCount = 0;
        public Disposable Disposable;
        public RequiresDisposable(Disposable disposable)
        {
            if (disposable == null) throw new ArgumentException(nameof(disposable));
            Disposable = disposable;
        }
        public void Dispose()
        {
            ++DisposedCount;
            Disposable.Dispose();
        }
    }
}
