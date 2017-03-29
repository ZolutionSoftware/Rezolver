using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class EnumerableExamples
    {
        [Fact]
        public void ShouldInjectEmptyEnumerable()
        {
            //<example1>
            var container = new Container();
            container.RegisterType<RequiresEnumerableOfServices>();

            var result = container.Resolve<RequiresEnumerableOfServices>();
            Assert.Empty(result.Services);
            //</example1>
        }

        [Fact]
        public void ShouldInjectEnumerableWithThreeItems()
        {
            //<example2>
            var container = new Container();
            var expectedTypes = new[] {
                typeof(MyService1), typeof(MyService2), typeof(MyService3)
            };
            foreach(var t in expectedTypes)
            {
                container.RegisterType(t, typeof(IMyService));
            }
            container.RegisterType<RequiresEnumerableOfServices>();

            var result = container.Resolve<RequiresEnumerableOfServices>();
            Assert.Equal(3, result.Services.Count());
            Assert.All(
                result.Services.Zip(
                    expectedTypes, 
                    (s, t) => (service: s, expectedType: t)
                ),
                t => Assert.IsType(t.expectedType, t.service));
            //</example2>
        }

        [Fact]
        public void ShouldInjectEnumerableWithItemsFromDifferentTargets()
        {
            //<example3>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterDelegate<IMyService>(() => new MyService2());
            container.RegisterObject<IMyService>(new MyService3());

            //shows also that injection of IEnumerables holds wherever injection
            //is normally supported - such as here, with delegate argument injection
            container.RegisterDelegate((IEnumerable<IMyService> services) =>
            {
                //if MyService4 is missing, add it to the enumerable
                if (!services.OfType<MyService4>().Any())
                    services = services.Concat(new[] { new MyService4() });
                return new RequiresEnumerableOfServices(services);
            });

            var result = container.Resolve<RequiresEnumerableOfServices>();

            Assert.Equal(4, result.Services.Count());
            //just check they're all different types this time.
            Assert.Equal(4, result.Services.Select(s => s.GetType()).Distinct().Count());
            //</example3>
        }

        [Fact]
        public void ShouldInjectEnumerableWithItemsWithDifferentLifetimes()
        {
            //<example4>
            //since we're using a scoped registration here,
            //we'll use the ScopedContainer, which establishes
            //a root scope.
            var container = new ScopedContainer();

            container.RegisterSingleton<MyService1, IMyService>();
            container.RegisterScoped<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            //So - each enumerable will contain, in order:
            // 1) Singleton IMyService
            // 2) Scoped IMyService
            // 3) Transient IMyService

            var fromRoot1 = container.Resolve<IEnumerable<IMyService>>().ToArray();
            var fromRoot2 = container.Resolve<IEnumerable<IMyService>>().ToArray();

            Assert.Same(fromRoot1[0], fromRoot2[0]);
            //both scoped objects should be the same because we've resolved
            //from the root scope
            Assert.Same(fromRoot1[1], fromRoot2[1]);
            Assert.NotSame(fromRoot1[2], fromRoot2[2]);

            using(var childScope = container.CreateScope())
            {
                var fromChildScope1 = childScope.Resolve<IEnumerable<IMyService>>().ToArray();
                //singleton should be the same as before, but 
                //the scoped object will be different
                Assert.Same(fromRoot1[0], fromChildScope1[0]);
                Assert.NotSame(fromRoot1[1], fromChildScope1[1]);
                Assert.NotSame(fromRoot1[2], fromChildScope1[2]);

                var fromChildScope2 = childScope.Resolve<IEnumerable<IMyService>>().ToArray();
                //the scoped object will be the same as above
                Assert.Same(fromChildScope1[0], fromChildScope2[0]);
                Assert.Same(fromChildScope1[1], fromChildScope2[1]);
                Assert.NotSame(fromChildScope1[2], fromChildScope2[2]);
            }
            //</example4>
        }

        [Fact]
        public void ShouldResolveEnumerableOfOpenGenerics()
        {
            //<example5>
            //USE GenericServiceContainer here - although do away with the constructor argument and property - give it
            // a method.  Then add another container implementation; register both and resolve two.
            //</example5>
        }
    }
}
