namespace Rezolver.Tests.Types
{
    public class RequiresScopeAndDisposable2 : IDisposable
    {
        public IContainerScope Scope { get; }
        public Disposable Disposable { get; }
        public RequiresScopeAndDisposable3 Next { get; }
        public RequiresScopeAndDisposable2(IContainerScope scope, Disposable disposable, RequiresScopeAndDisposable3 next)
        {
            Scope = scope;
            Disposable = disposable;
            Next = next;
        }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}