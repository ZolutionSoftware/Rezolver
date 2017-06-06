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
            // <example1>
            var container = new Container();
            container.RegisterType<RequiresEnumerableOfServices>();

            var result = container.Resolve<RequiresEnumerableOfServices>();
            Assert.Empty(result.Services);
            // </example1>
        }

        [Fact]
        public void ShouldInjectEnumerableWithThreeItems()
        {
            // <example2>
            var container = new Container();
            var expectedTypes = new[] {
                typeof(MyService1), typeof(MyService2), typeof(MyService3)
            };
            foreach (var t in expectedTypes)
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
            // </example2>
        }

        [Fact]
        public void ShouldInjectEnumerableWithItemsFromDifferentTargets()
        {
            // <example3>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterDelegate<IMyService>(() => new MyService2());
            container.RegisterObject<IMyService>(new MyService3());

            // shows also that injection of IEnumerables holds wherever injection
            // is normally supported - such as here, with delegate argument injection
            container.RegisterDelegate((IEnumerable<IMyService> services) =>
            {
                // if MyService4 is missing, add it to the enumerable
                if (!services.OfType<MyService4>().Any())
                    services = services.Concat(new[] { new MyService4() });
                return new RequiresEnumerableOfServices(services);
            });

            var result = container.Resolve<RequiresEnumerableOfServices>();

            Assert.Equal(4, result.Services.Count());
            // just check they're all different types this time.
            Assert.Equal(4, result.Services.Select(s => s.GetType()).Distinct().Count());
            // </example3>
        }

        [Fact]
        public void ShouldInjectEnumerableWithItemsWithDifferentLifetimes()
        {
            // <example4>
            // since we're using a scoped registration here,
            // we'll use the ScopedContainer, which establishes
            // a root scope.
            var container = new ScopedContainer();

            container.RegisterSingleton<MyService1, IMyService>();
            container.RegisterScoped<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            // So - each enumerable will contain, in order:
            // 1) Singleton IMyService
            // 2) Scoped IMyService
            // 3) Transient IMyService

            var fromRoot1 = container.Resolve<IEnumerable<IMyService>>().ToArray();
            var fromRoot2 = container.Resolve<IEnumerable<IMyService>>().ToArray();

            Assert.Same(fromRoot1[0], fromRoot2[0]);
            // both scoped objects should be the same because we've resolved
            // from the root scope
            Assert.Same(fromRoot1[1], fromRoot2[1]);
            Assert.NotSame(fromRoot1[2], fromRoot2[2]);

            using (var childScope = container.CreateScope())
            {
                var fromChildScope1 = childScope.Resolve<IEnumerable<IMyService>>().ToArray();
                // singleton should be the same as before, but 
                // the scoped object will be different
                Assert.Same(fromRoot1[0], fromChildScope1[0]);
                Assert.NotSame(fromRoot1[1], fromChildScope1[1]);
                Assert.NotSame(fromRoot1[2], fromChildScope1[2]);

                var fromChildScope2 = childScope.Resolve<IEnumerable<IMyService>>().ToArray();
                // the scoped object will be the same as above
                Assert.Same(fromChildScope1[0], fromChildScope2[0]);
                Assert.Same(fromChildScope1[1], fromChildScope2[1]);
                Assert.NotSame(fromChildScope1[2], fromChildScope2[2]);
            }
            // </example4>
        }

        [Fact]
        public void ShouldResolveEnumerableOfOpenGenerics()
        {
            // <example5>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterType(typeof(UsesAnyService2<>), typeof(IUsesAnyService<>));

            var result = container.Resolve<IEnumerable<IUsesAnyService<IMyService>>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<UsesAnyService<IMyService>>(result[0]);
            Assert.IsType<UsesAnyService2<IMyService>>(result[1]);
            // </example5>
        }

        [Fact]
        public void ShouldGenerateEnumerableOfAllMatchingOpenAndClosedGenerics()
        {
            // <example6>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterType<UsesIMyService, IUsesAnyService<IMyService>>();
            container.RegisterType<UsesIMyService2, IUsesAnyService<IMyService>>();

            var result = container.Resolve<IEnumerable<IUsesAnyService<IMyService>>>().ToArray();
            var result2 = container.Resolve<IEnumerable<IUsesAnyService<MyService>>>().ToArray();

            //note the order - specific generic type matches first, followed by more general
            Assert.Equal(3, result.Length);
            Assert.IsType<UsesIMyService>(result[0]);
            Assert.IsType<UsesIMyService2>(result[1]);
            Assert.IsType<UsesAnyService<IMyService>>(result[2]);

            Assert.Equal(1, result2.Length);
            Assert.IsType<UsesAnyService<MyService>>(result2[0]);
            // </example6>
        }

        [Fact]
        public void ShouldResolveEnumerableOfClosedGenericsInsteadOfOpen()
        {
            // <example6b>
            var container = new Container();
            // same test - we're just setting an option on the container
            // which changes how generics are matched for the FetchAll() call
            // which sits behind the automatic enumerable resolving behaviour.
            container.SetOption<Options.FetchAllMatchingGenerics>(false);

            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterType<UsesIMyService, IUsesAnyService<IMyService>>();
            container.RegisterType<UsesIMyService2, IUsesAnyService<IMyService>>();

            // This time, this will only match the second two registrations which 
            // specialise for IUsesAnyService<IMyService>
            var result = container.Resolve<IEnumerable<IUsesAnyService<IMyService>>>().ToArray();
            var result2 = container.Resolve<IEnumerable<IUsesAnyService<MyService>>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<UsesIMyService>(result[0]);
            Assert.IsType<UsesIMyService2>(result[1]);

            Assert.Equal(1, result2.Length);
            Assert.IsType<UsesAnyService<MyService>>(result2[0]);
            // </example6b>
        }

        [Fact]
        public void ShouldResolveEnumerableOfDecoratedServices()
        {
            // <example7>
            var container = new Container();
            // register the decorator up front.  Note - it doesn't actually matter when it's registered
            container.RegisterDecorator<MyServiceDecorator1, IMyService>();
            container.RegisterType<MyService, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            // create the container with these targets
            var result = container.Resolve<IEnumerable<IMyService>>().ToArray();

            // make sure each item in the enumerable is an instance of our decorator.
            // then make sure the decorated services are correct.
            Assert.All(result, r => Assert.IsType<MyServiceDecorator1>(r));
            Assert.IsType<MyService>(((MyServiceDecorator1)result[0]).Inner);
            Assert.IsType<MyService2>(((MyServiceDecorator1)result[1]).Inner);
            Assert.IsType<MyService3>(((MyServiceDecorator1)result[2]).Inner);
            // </example7>
        }

        [Fact]
        public void ShouldResolveEnumerableViaExplicitRegistration()
        {
            // <example8>
            var container = new Container();
            container.RegisterType<MyService1>();
            container.RegisterType<MyService2>();
            container.RegisterDelegate<IEnumerable<IMyService>>(
                rc => new IMyService[] { rc.Resolve<MyService2>(), rc.Resolve<MyService1>() }
                );

            var result = container.Resolve<IEnumerable<IMyService>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<MyService1>(result[0]);
            Assert.IsType<MyService2>(result[1]);
            // </example8>
        }
    }
}
