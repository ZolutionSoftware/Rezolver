using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ScopedSingletonTests
    {
        private class ScopedObject
        {
            private static int _counter = 1;

            public int InstanceID = _counter++;

            public override string ToString()
            {
                return "ScopedSingletonObject Instance: " + InstanceID.ToString();
            }
        }

        [Fact]
        public void ShouldCreateOneInstanceOfScopedService()
        {
            var resolver = new DefaultLifetimeScopeRezolver();
            resolver.RegisterScoped<ScopedObject>();

            var instance1 = resolver.Resolve<ScopedObject>();
            var instance2 = resolver.Resolve<ScopedObject>();

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void ChildScopeShouldCreateNewInstanceOfScopedService()
        {
            //using block here is not really needed, of course, as we're not 
            //creating a disposable.
            using (var resolver = new DefaultLifetimeScopeRezolver())
            {
                resolver.RegisterScoped<ScopedObject>();

                var instance1 = resolver.Resolve<ScopedObject>();

                using (var childScope = resolver.CreateLifetimeScope())
                {
                    var instance2 = childScope.Resolve<ScopedObject>();

                    Assert.NotSame(instance1, instance2);

                    var instance2a = childScope.Resolve<ScopedObject>();
                    Assert.Same(instance2, instance2a);
                }

                //and then re-resolve on outer scope (ensures that child scope does not pollute 
                //parent scope)
                var instance1a = resolver.Resolve<ScopedObject>();
                Assert.Same(instance1, instance1a);
            }
        }
    }
}
