using System;

namespace Rezolver.Tests.Types
{
    public class RequiresScopeAndDisposable2 : IDisposable
    {
        public ContainerScope Scope { get; }
        public Disposable Disposable { get; }
        public RequiresScopeAndDisposable3 Next { get; }
        public RequiresScopeAndDisposable2(ContainerScope scope, Disposable2 disposable, RequiresScopeAndDisposable3 next)
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