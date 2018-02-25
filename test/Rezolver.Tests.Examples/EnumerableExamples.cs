// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

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

            var fromRoot1 = container.ResolveMany<IMyService>().ToArray();
            var fromRoot2 = container.ResolveMany<IMyService>().ToArray();

            Assert.Same(fromRoot1[0], fromRoot2[0]);
            // both scoped objects should be the same because we've resolved
            // from the root scope
            Assert.Same(fromRoot1[1], fromRoot2[1]);
            Assert.NotSame(fromRoot1[2], fromRoot2[2]);

            using (var childScope = container.CreateScope())
            {
                var fromChildScope1 = childScope.ResolveMany<IMyService>().ToArray();
                // singleton should be the same as before, but 
                // the scoped object will be different
                Assert.Same(fromRoot1[0], fromChildScope1[0]);
                Assert.NotSame(fromRoot1[1], fromChildScope1[1]);
                Assert.NotSame(fromRoot1[2], fromChildScope1[2]);

                var fromChildScope2 = childScope.ResolveMany<IMyService>().ToArray();
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

            var result = container.ResolveMany<IUsesAnyService<IMyService>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<UsesAnyService<IMyService>>(result[0]);
            Assert.IsType<UsesAnyService2<IMyService>>(result[1]);
            // </example5>
        }

        [Fact]
        public void ShouldResolveEnumerableOfConstrainedGenerics()
        {
            // <example5b>
            var container = new Container();
            container.RegisterType(typeof(GenericAny<>), typeof(IGeneric<>));
            container.RegisterType(typeof(GenericAnyIMyService<>), typeof(IGeneric<>));
            container.RegisterType(typeof(GenericAnyMyService1<>), typeof(IGeneric<>));

            var anyResult = container.ResolveMany<IGeneric<string>>().ToArray();
            var myServiceResult = container.ResolveMany<IGeneric<MyService>>().ToArray();
            var myService1Result = container.ResolveMany<IGeneric<MyService1>>().ToArray();

            // only the first registration matches IGeneric<string>
            Assert.Single(anyResult);
            Assert.IsType<GenericAny<string>>(anyResult[0]);

            // First and second registrations match IGeneric<MyService>
            // Note the order: both registrations were IGeneric<> so the constrained generic
            // appears after the non-constrained one.
            Assert.Equal(2, myServiceResult.Length);
            Assert.IsType<GenericAny<MyService>>(myServiceResult[0]);
            Assert.IsType<GenericAnyIMyService<MyService>>(myServiceResult[1]);

            // All registrations match and, again, all are returned in order.
            Assert.Equal(3, myService1Result.Length);
            Assert.IsType<GenericAny<MyService1>>(myService1Result[0]);
            Assert.IsType<GenericAnyIMyService<MyService1>>(myService1Result[1]);
            Assert.IsType<GenericAnyMyService1<MyService1>>(myService1Result[2]);
            // </example5b>
        }

        [Fact]
        public void ShouldGenerateEnumerableOfAllMatchingOpenAndClosedGenerics()
        {
            // <example6>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterType<UsesIMyService, IUsesAnyService<IMyService>>();
            container.RegisterType<UsesIMyService2, IUsesAnyService<IMyService>>();

            var result = container.ResolveMany<IUsesAnyService<IMyService>>().ToArray();
            var result2 = container.ResolveMany<IUsesAnyService<MyService>>().ToArray();

            //Note - enumerable is in registration order, regardless of open/closed generic
            Assert.Equal(3, result.Length);
            Assert.IsType<UsesAnyService<IMyService>>(result[0]);
            Assert.IsType<UsesIMyService>(result[1]);
            Assert.IsType<UsesIMyService2>(result[2]);

            Assert.Single(result2);
            Assert.IsType<UsesAnyService<MyService>>(result2[0]);
            // </example6>
        }

        [Fact]
        public void ShouldResolveOnlyBestMatchBecauseOfGlobalOption()
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
            var result = container.ResolveMany<IUsesAnyService<IMyService>>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<UsesIMyService>(result[0]);
            Assert.IsType<UsesIMyService2>(result[1]);
            // </example6b>
        }

        [Fact]
        public void ShouldResolveOnlyBestMatchForOneServiceTypeBecauseOfLocalOption()
        {
            // <example6c>
            var container = new Container();
            // set this option for the IGeneric<MyService2> service ONLY
            container.SetOption<Options.FetchAllMatchingGenerics>(false, typeof(IGeneric<MyService2>));

            // same registrations as our constrained generics example plus an 
            // extra for IGeneric<MyService2> which will 'win'.
            container.RegisterType(typeof(GenericAny<>), typeof(IGeneric<>));
            container.RegisterType(typeof(GenericAnyIMyService<>), typeof(IGeneric<>));
            container.RegisterType(typeof(GenericAnyMyService1<>), typeof(IGeneric<>));
            container.RegisterType(typeof(GenericMyService2), typeof(IGeneric<MyService2>));

            // will get two as before
            var myServiceResult = container.ResolveMany<IGeneric<MyService>>().ToArray();
            // would normally get three (via 1st, 2nd and 4th registrations), but will only
            // get one.
            var myService2Result = container.ResolveMany<IGeneric<MyService2>>().ToArray();

            Assert.Equal(2, myServiceResult.Length);
            Assert.Single(myService2Result);
            // </example6c>
            // NOTE ABOVE - OMITTING THE INDIVIDUAL ITEM CHECKS BECAUSE IT JUST REPEATS THE CONSTRAINTS TEST
        }

        [Fact]
        public void ShouldDisableAllOpenGenericsForOneType()
        {
            // <example6d>
            var container = new Container();
            container.SetOption<Options.FetchAllMatchingGenerics>(false, typeof(IGeneric<>));

            // similar to the constraints example and the previous one, except this time we
            // only have one open generic registration, and the other two that we have are closed
            // (for IGeneric<MyService1> and IGeneric<MyService2>)
            container.RegisterType(typeof(GenericAny<>), typeof(IGeneric<>));
            // just reusing this open generic as a closed generic
            container.RegisterType(typeof(GenericAnyMyService1<MyService1>), typeof(IGeneric<MyService1>));
            container.RegisterType(typeof(GenericMyService2), typeof(IGeneric<MyService2>));

            // ordinarily, both of these would return two results because of the IGeneric<> open
            // registration.  But because the FetchAll behaviour has been disabled for all IGeneric<>
            // types, both only get one result.
            var myService1Result = container.ResolveMany<IGeneric<MyService1>>().ToArray();
            var myService2Result = container.ResolveMany<IGeneric<MyService2>>().ToArray();

            Assert.Single(myService1Result);
            Assert.Single(myService2Result);
            Assert.NotEqual(myService1Result[0].GetType(), myService2Result[0].GetType());
            // </example6d>
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
            var result = container.ResolveMany<IMyService>().ToArray();

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

            // reversing the 'normal' order that would usually be 
            // produced by the default IEnumerable functionality, to show
            // that it's this enumerable that we resolve
            container.RegisterDelegate<IEnumerable<IMyService>>(
                rc => new IMyService[] { rc.Resolve<MyService2>(), rc.Resolve<MyService1>() }
            );

            var result = container.ResolveMany<IMyService>().ToArray();

            Assert.Equal(2, result.Length);
            Assert.IsType<MyService2>(result[0]);
            Assert.IsType<MyService1>(result[1]);
            // </example8>
        }

        [Fact]
        public void LazyEnumerable()
        {
            // <example9>
            var container = new Container();
            var instanceCounter = 0;

            // ensures we get an enumerable with three items
            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();

            container.RegisterObject<Action<CallsYouBackOnCreate>>(
                o => ++instanceCounter);

            var items = container.ResolveMany<CallsYouBackOnCreate>();

            // start by asserting that no instances have been created yet
            Assert.Equal(0, instanceCounter);

            var lastCounter = instanceCounter;
            foreach(var item in items)
            {
                // every time we move next, a new item should be created,
                // which, in turn, fires the delegate which increments the 
                // counter
                Assert.Equal(lastCounter + 1, instanceCounter);
                lastCounter = instanceCounter;
            }

            // more importantly - if we enumerate it again, then the 
            // objects are created again
            foreach (var item in items)
            {
                Assert.Equal(lastCounter + 1, instanceCounter);
                lastCounter = instanceCounter;
            }
            // </example9>
        }

        [Fact]
        public void EagerEnumerable_Global()
        {
            // <example10>
            var container = new Container();
            var instanceCounter = 0;

            // set this option to disable lazy enumerables globally
            container.SetOption<Options.LazyEnumerables>(false);

            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();

            container.RegisterObject<Action<CallsYouBackOnCreate>>(
                o => ++instanceCounter);

            var items = container.ResolveMany<CallsYouBackOnCreate>();

            // this time all instances will be created immediately.
            Assert.Equal(3, instanceCounter);

            // and we'll just assert that the instance count never changes
            foreach(var item in items)
            {
                Assert.Equal(3, instanceCounter);
            }

            foreach (var item in items)
            {
                Assert.Equal(3, instanceCounter);
            }
            // </example10>
        }

        [Fact]
        public void EagerEnumerable_PerService()
        {
            // <example11>
            // for this test we'll drop the two foreach loops and just use .ToArray()
            var container = new Container();
            var instanceCounter1 = 0;
            var instanceCounter2 = 0;

            // set this option to disable lazy enumerables only for 
            // the type 'CallsYouBackOnCreate2'
            container.SetOption<Options.LazyEnumerables, CallsYouBackOnCreate2>(false);

            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterType<CallsYouBackOnCreate>();
            container.RegisterObject<Action<CallsYouBackOnCreate>>(
                o => ++instanceCounter1);

            container.RegisterType<CallsYouBackOnCreate2>();
            container.RegisterType<CallsYouBackOnCreate2>();
            container.RegisterType<CallsYouBackOnCreate2>();
            container.RegisterObject<Action<CallsYouBackOnCreate2>>(
                o => ++instanceCounter2);

            // will be lazy
            var items1 = container.ResolveMany<CallsYouBackOnCreate>();
            Assert.Equal(0, instanceCounter1);

            // will be eager
            var items2 = container.ResolveMany<CallsYouBackOnCreate2>();
            Assert.Equal(3, instanceCounter2);

            var array1a = items1.ToArray();
            var array1b = items1.ToArray();
            var array2a = items2.ToArray();
            var array2b = items2.ToArray();

            Assert.Equal(6, instanceCounter1);
            Assert.Equal(3, instanceCounter2);
            // </example11>
        }

        [Fact]
        public void ShouldDisableEnumerables()
        {
            // <example12>
            var config = TargetContainer.DefaultConfig.Clone();
            config.ConfigureOption<Options.EnableEnumerableInjection>(false);

            // pass our custom config to a new target container which, in turn,
            // we pass to the constructor of the Container class
            var container = new Container(new TargetContainer(config));

            Assert.Throws<InvalidOperationException>(() => container.ResolveMany<int>());
            // </example12>
        }

        [Fact]
        public void ShouldProjectSimplePriceAdjustments()
        {
            // <example100>
            var container = new Container();

            container.RegisterObject(new SimplePriceAdjustmentConfig()
            {
                Adjustment = 10M,
                DisplayName = "Always Add 10"
            });

            container.RegisterObject(new SimplePriceAdjustmentConfig()
            {
                Adjustment = 0.75M,
                DisplayName = "25% Off",
                IsPercentage = true,
                TriggerPrice = 49.99M
            });

            // now register the projection (note: it can be set up
            // at any time, and additional registrations can be made
            // which match the source enumerable after this registration
            // is done)
            container.RegisterProjection<SimplePriceAdjustmentConfig, SimplePriceAdjustment>();

            container.RegisterType<SimplePriceCalculator>();

            // get our calculator
            var calc = container.Resolve<SimplePriceCalculator>();

            Assert.Equal(40 + 10, calc.Calculate(40));
            Assert.Equal((55 + 10) * 0.75M, calc.Calculate(55));
            // </example100>
        }

        [Fact]
        public void ShouldProjectDecoratedAdjustments()
        {
            // <example101>
            var container = new Container();
            container.RegisterType<SimplePriceAdjustment, IPriceAdjustment>();
            container.RegisterDecorator<NeverLessThanHalfPrice, IPriceAdjustment>();
            container.RegisterType<PriceCalculator>();
            
            // note here - projection targets IPriceAdjustment now
            container.RegisterProjection<SimplePriceAdjustmentConfig, IPriceAdjustment>();

            container.RegisterObject(new SimplePriceAdjustmentConfig()
            {
                Adjustment = -10M,
                DisplayName = "10 off"
            });

            container.RegisterObject(new SimplePriceAdjustmentConfig()
            {
                Adjustment = 0.75M,
                IsPercentage = true,
                DisplayName = "25% off"
            });

            var calculator = container.Resolve<PriceCalculator>();

            // 25% off will not be applied.
            Assert.Equal(10, calculator.Calculate(20));
            // but it is here
            Assert.Equal(15, calculator.Calculate(30));
            // </example101>
        }
    }
}
