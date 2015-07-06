using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.vNext
{
    public class ObjectTargetTests
    {
        private class MyDisposable : IDisposable
        {

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    disposedValue = true;
                }
                else
                    throw new ObjectDisposedException("MyDisposable");
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
            }
            #endregion

        }

        [Fact]
        public void ShouldNotDisposeByDefault()
        {
            var myDisposable = new MyDisposable();
            using (var rezolver = new DefaultLifetimeScopeRezolver())
            {
                rezolver.RegisterObject(myDisposable);
                var instance = rezolver.Resolve<MyDisposable>();
            }

            myDisposable.Dispose();
        }

        [Fact]
        public void ShouldNotDisposeMultipleTimes()
        {
            var myDisposable = new MyDisposable();
            //test targeted specifically at a piece of functionality I currently know not to work.
            //an objecttarget should behave like a SingletonTarget in terms of how it tracks in a scope.
            using (var rezolver = new DefaultLifetimeScopeRezolver())
            {
                rezolver.RegisterObject(myDisposable, suppressScopeTracking: false);
                using(var childScope = rezolver.CreateLifetimeScope())
                {
                    var instance = childScope.Resolve<MyDisposable>();
                    //should not dispose here when scope is disposed
                }
                var instance2 = rezolver.Resolve<MyDisposable>();
                //should dispose here when root scope is disposed
            }

            //should already be disposed here.
            Assert.Throws<ObjectDisposedException>(() => myDisposable.Dispose());
        }
    }
}
