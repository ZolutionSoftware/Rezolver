using System;

namespace Rezolver.Tests.Types
{
    public class RequiresScopeAndDisposable3 : IDisposable
    {
        public IContainerScope Scope { get; }
        public Disposable Disposable { get; }
        public RequiresScopeAndDisposable3(IContainerScope scope, Disposable3 disposable)
        {
            Scope = scope;
            Disposable = disposable;
        }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}