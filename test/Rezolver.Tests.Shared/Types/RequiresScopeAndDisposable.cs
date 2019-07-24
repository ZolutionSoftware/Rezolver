using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresScopeAndDisposable : IDisposable
    {
        public ContainerScope Scope { get; }
        public Disposable Disposable { get; }
        public RequiresScopeAndDisposable2 Next { get; }
        public RequiresScopeAndDisposable(ContainerScope scope, Disposable disposable, RequiresScopeAndDisposable2 next)
        {
            Scope = scope;
            Disposable = disposable;
            Next = next;
        }

        public void Dispose()
        {
            //only cascade disposal of the object - the scope should be disposed by its parent scope
            Disposable.Dispose();
        }
    }
}
