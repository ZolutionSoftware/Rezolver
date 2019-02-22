using Rezolver.Options;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class AutoLazyExamples
    {
        private interface IFoo { }
        private class Foo : IFoo
        {
            public static int InstanceCount = 0;

            public Foo() { ++InstanceCount; }
        }

        // This example is purely to show what doesn't work.
        // [Fact]
        public void DoesntWork()
        {
            int last = Foo.InstanceCount;

            var container = new Container();

            // <example0>
            container.RegisterType<Foo, IFoo>();
            container.RegisterAutoFunc<IFoo>();
            container.RegisterType<Lazy<IFoo>>();

            var result = container.Resolve<Lazy<IFoo>>();
            // </example0>

            Assert.Equal(last, Foo.InstanceCount);
        }

        private Container CreateContainerWithOnlyAutoLazy()
        {
            // <example1a>
            // This shows creating a new configuration
            // but it can also be applied directly to the DefaultConfig.
            var config = TargetContainer
                .DefaultConfig
                .Clone()
                .ConfigureOption<EnableAutoLazyInjection>(true);

            var container = new Container(new TargetContainer(config));
            // </example1a>
            return container;
        }

        [Fact]
        public void ShouldEnable()
        {
            var container = CreateContainerWithOnlyAutoLazy();

            Assert.True(container.CanResolve<Lazy<int>>());
        }

        [Fact]
        public void ShouldCreateInstanceLazily()
        {
            int last = Foo.InstanceCount;

            var container = CreateContainerWithOnlyAutoLazy();

            // <example1b>
            container.RegisterType<Foo, IFoo>();
            container.RegisterAutoFunc<IFoo>();

            var result = container.Resolve<Lazy<IFoo>>();
            // </example1b>

            Assert.Equal(last, Foo.InstanceCount);
            var instance = result.Value;
            Assert.Equal(last + 1, Foo.InstanceCount);
        }

        private Container CreateContainerWithAutoFactoriesAndLazies()
        {
            // <example2a>
            // This shows creating a new configuration
            // but it can also be applied directly to the DefaultConfig.
            var config = TargetContainer
                .DefaultConfig
                .Clone()
                .ConfigureOption<EnableAutoFuncInjection>(true)
                .ConfigureOption<EnableAutoLazyInjection>(true);

            var container = new Container(new TargetContainer(config));
            // </example2a>
            return container;
        }

        [Fact]
        public void ShouldCreateInstanceLazily_WithoutRegisteringAutoFunc()
        {
            int last = Foo.InstanceCount;

            var container = CreateContainerWithAutoFactoriesAndLazies();

            // <example2b>
            container.RegisterType<Foo, IFoo>();

            var result = container.Resolve<Lazy<IFoo>>();
            // </example2b>

            Assert.Equal(last, Foo.InstanceCount);
            var instance = result.Value;
            Assert.Equal(last + 1, Foo.InstanceCount);
        }

        [Fact]
        public void ShouldHonourScopes()
        {
            var container = CreateContainerWithAutoFactoriesAndLazies();

            // <example3>
            container.RegisterScoped<MyService, IMyService>();
            container.RegisterType<DisposableType>();

            DisposableType outerDisposable, innerDisposable;

            using (var scope = container.CreateScope())
            {
                // get two lazies for IMyService
                var l1 = scope.Resolve<Lazy<IMyService>>();
                var l2 = scope.Resolve<Lazy<IMyService>>();
                var l3 = scope.Resolve<Lazy<DisposableType>>();

                // different lazies...
                Assert.NotSame(l1, l2);

                var service1 = l1.Value;
                var service2 = l2.Value;

                //... but same instances because MyService was registered as Scoped.
                Assert.Same(service1, service2);

                outerDisposable = l3.Value;

                using (var childScope = scope.CreateScope())
                {
                    var l4 = childScope.Resolve<Lazy<DisposableType>>();
                    innerDisposable = l4.Value;
                }

                Assert.True(innerDisposable.Disposed);
                Assert.False(outerDisposable.Disposed);
            }
            Assert.True(outerDisposable.Disposed);
            // </example3>
        }
    }
}
