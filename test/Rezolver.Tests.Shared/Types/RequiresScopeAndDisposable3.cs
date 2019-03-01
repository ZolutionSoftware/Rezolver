using System;

namespace Rezolver.Tests.Types
{
    public class RequiresScopeAndDisposable3 : IDisposable
    {
        public ContainerScope2 Scope { get; }
        public Disposable Disposable { get; }
        public RequiresScopeAndDisposable3(ContainerScope2 scope, Disposable3 disposable)
        {
            Scope = scope;
            Disposable = disposable;
        }

        public void Dispose()
        {
            //only cascade disposal of the object - the scope should be disposed by its parent scope
            Disposable.Dispose();
        }
    }
}