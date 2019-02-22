using Rezolver.Options;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class AutoFactoryExamples
    {
        [Fact]
        public void ShouldProduceAutoFactory()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterAutoFunc<IMyService>();

            var factory = container.Resolve<Func<IMyService>>();
            var instance = factory();

            Assert.IsType<MyService1>(instance);
            // </example1>
        }

        [Fact]
        public void ShouldProduceAFactoryWhichCreatesAnEnumerable()
        {
            var container = new Container();
            // <example2>
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();
            container.RegisterAutoFunc<IEnumerable<IMyService>>();

            var factory = container.Resolve<Func<IEnumerable<IMyService>>>();
            var instances = factory();  // will contain three instances
            // </example2>

            var array = instances.ToArray();
            Assert.IsType<MyService1>(array[0]);
            Assert.IsType<MyService2>(array[1]);
            Assert.IsType<MyService3>(array[2]);
        }

        [Fact]
        public void ShouldProduceAnEnumerableOfFactories()
        {
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            // <example3>
            container.RegisterAutoFunc<IMyService>();

            // will contain three funcs, one for each registration compatible with
            // IMyService, thanks to enumerable covariance.
            var factories = container.ResolveMany<Func<IMyService>>();
            var instances = factories.Select(f => f());
            // </example3>

            var array = instances.ToArray();
            Assert.IsType<MyService1>(array[0]);
            Assert.IsType<MyService2>(array[1]);
            Assert.IsType<MyService3>(array[2]);
        }

        // <example4a>
        public delegate IMyService MyServiceFactory();
        // </example4a>

        [Fact]
        public void ShouldProduceAnEnumerableOfCustomFactories()
        {
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            // <example4b>
            container.RegisterAutoFactory<MyServiceFactory>();

            var factories = container.ResolveMany<MyServiceFactory>();
            var instances = factories.Select(f => f());
            // </example4b>

            var array = instances.ToArray();
            Assert.IsType<MyService1>(array[0]);
            Assert.IsType<MyService2>(array[1]);
            Assert.IsType<MyService3>(array[2]);
        }

        [Fact]
        public void ShouldUseArgumentToOverrideDependency()
        {
            var container = new Container();
            // <example5>
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<RequiresIMyService, IRequiresIMyService>();
            container.RegisterAutoFunc<IMyService, IRequiresIMyService>();

            var factory = container.Resolve<Func<IMyService, IRequiresIMyService>>();
            var myService2 = new MyService2();

            var instance = factory(myService2);

            Assert.Same(instance.Service, myService2);
            // </example5>
        }

        [Fact]
        public void ShouldUseArgumentToSupplyDependency()
        {
            var container = new Container();

            // <example6>
            container.RegisterType<RequiresIMyService, IRequiresIMyService>();
            container.RegisterAutoFunc<IMyService, IRequiresIMyService>();

            var factory = container.Resolve<Func<IMyService, IRequiresIMyService>>();
            var myService2 = new MyService2();

            var instance = factory(myService2);
            // </example6>

            Assert.Same(instance.Service, myService2);
        }

        [Fact]
        public void FactoriesShouldHonourScopes()
        {
            var container = new Container();

            // <example7>
            container.RegisterScoped<MyService, IMyService>();
            container.RegisterType<DisposableType>();
            container.RegisterAutoFunc<IMyService>();
            container.RegisterAutoFunc<DisposableType>();

            DisposableType outerDisposable, innerDisposable;

            using (var scope = container.CreateScope())
            {
                // get two funcs for IMyService
                var f1 = scope.Resolve<Func<IMyService>>();
                var f2 = scope.Resolve<Func<IMyService>>();
                var f3 = scope.Resolve<Func<DisposableType>>();

                // different funcs...
                Assert.NotSame(f1, f2);

                var service1 = f1();
                var service2 = f2();

                //... but same instances because MyService was registered as Scoped.
                Assert.Same(service1, service2); 

                outerDisposable = f3();

                using(var childScope = scope.CreateScope())
                {
                    var f4 = childScope.Resolve<Func<DisposableType>>();
                    innerDisposable = f4();
                }

                Assert.True(innerDisposable.Disposed);
                Assert.False(outerDisposable.Disposed);
            }
            Assert.True(outerDisposable.Disposed);
            // </example7>
        }

        // <example8a>
        public interface IWrapper<T>
        {
            T InnerValue { get; }
        }

        public class Wrapper<T> : IWrapper<T>
        {
            public Wrapper(T innerValue)
            {
                InnerValue = innerValue;
            }

            public T InnerValue { get; }
        }
        // </example8a>

        [Fact]
        public void ShouldProduceClosedGeneric()
        {
            var container = new Container();

            // <example8b>
            container.RegisterObject(10);
            container.RegisterObject("hello world");
            container.RegisterType(typeof(Wrapper<>), typeof(IWrapper<>));
            container.RegisterAutoFactory(typeof(Func<>).MakeGenericType(typeof(IWrapper<>)));

            var f1 = container.Resolve<Func<IWrapper<int>>>();
            var f2 = container.Resolve<Func<IWrapper<string>>>();

            var w1 = f1();
            var w2 = f2();

            Assert.Equal(10, w1.InnerValue);
            Assert.Equal("hello world", w2.InnerValue);
            // </example8b>
        }


        //[Fact]
#pragma warning disable xUnit1013 // Public method should be marked as test
        public void ShouldAutoRegisterFunc_ViaGlobalConfig()
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            // note - this isn't a test, because changing the global default configuration for all TargetContainers
            // would break a bunch of tests!
            // <example9a>
            TargetContainer
                .DefaultConfig
                .ConfigureOption<EnableAutoFuncInjection>(true);

            var container = new Container();
            container.RegisterObject("Hello world");

            var f1 = container.Resolve<Func<string>>();

            Assert.Equal("Hello world", f1());
            // </example9a>
        }

        [Fact]
        public void ShouldAutoRegisterFunc_ViaNewConfig()
        {
            // <example9b>
            var config = TargetContainer
                .DefaultConfig
                .Clone()    // clone the configuration
                .ConfigureOption<EnableAutoFuncInjection>(true);

            // create the container but pass a new TargetContainer 
            // to it which has been passed the configuration.
            var container = new Container(new TargetContainer(config));

            container.RegisterObject("Hello world");

            var f1 = container.Resolve<Func<string>>(); // f1() == "Hello World"
            // </example9b>
            Assert.Equal("Hello world", f1());
        }
    }
}
