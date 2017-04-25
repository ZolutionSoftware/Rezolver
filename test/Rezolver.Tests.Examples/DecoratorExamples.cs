using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class DecoratorExamples
    {
        [Fact]
        public void ShouldResolveOneDecoratedService()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            container.RegisterDecorator<MyServiceDecorator1, IMyService>();
            
            var result = container.Resolve<IMyService>();
            var decorator = Assert.IsType<MyServiceDecorator1>(result);

            Assert.IsType<MyService>(decorator.Inner);
            // </example1>
        }

        [Fact]
        public void ShouldResolveOneRedecoratedService()
        {
            // <example2>
            // same as before - but two decorators
            // note that the order they're registered determines the order in which 
            // the decoration occurs.
            var container = new Container();
            container.RegisterType<MyService, IMyService>();
            container.RegisterDecorator<MyServiceDecorator2, IMyService>();
            container.RegisterDecorator<MyServiceDecorator1, IMyService>();

            var result = container.Resolve<IMyService>();
            var decorator = Assert.IsType<MyServiceDecorator1>(result);
            var innerDecorator = Assert.IsType<MyServiceDecorator2>(decorator.Inner);

            Assert.IsType<MyService>(innerDecorator.Inner);
            // </example2>
        }

        [Fact]
        public void ShouldResolveMultiplRedecoratedServices()
        {
            // <example3>
            var container = new Container();
            // this time we'll register the decorators first, not because
            // we have to, but because we can :)
            container.RegisterDecorator<MyServiceDecorator2, IMyService>();
            container.RegisterDecorator<MyServiceDecorator1, IMyService>();

            // note: this array of types is purely to simplify the asserts
            // used at the end.
            var serviceTypes = new[] {
                typeof(MyService1), typeof(MyService2),
                typeof(MyService3), typeof(MyService4),
                typeof(MyService5), typeof(MyService6)
            };

            var serviceTargets = serviceTypes.Select(
                t => Target.ForType(t)
            );

            // another way to bulk-register multiple targets
            // against the same service type.  
            container.RegisterMultiple(serviceTargets, typeof(IMyService));

            var result = container.Resolve<IEnumerable<IMyService>>();

            Assert.All(serviceTypes.Zip(result, (t, s) => (type: t, service: s)),
                ts => {
                    var decorator = Assert.IsType<MyServiceDecorator1>(ts.service);
                    var innerDecorator = Assert.IsType<MyServiceDecorator2>(decorator.Inner);
                    Assert.IsType(ts.type, innerDecorator.Inner);
                });
            // </example3>
        }

        [Fact]
        public void ShouldResolveDecoratedGeneric()
        {
            // <example4>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterDecorator(typeof(UsesAnyServiceDecorator<>), typeof(IUsesAnyService<>));

            var result = container.Resolve<IUsesAnyService<MyService>>();

            var decorator = Assert.IsType<UsesAnyServiceDecorator<MyService>>(result);
            Assert.IsType<UsesAnyService<MyService>>(decorator.Inner);
            // </example4>
        }

        [Fact]
        public void ShouldUseSpecialisedDecoratorForMyService2()
        {
            //Fixed after BUG #27: https://github.com/ZolutionSoftware/Rezolver/issues/27

            // <example5>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterDecorator(typeof(UsesAnyServiceDecorator<>), typeof(IUsesAnyService<>));
            // this decorator only kicks in when resolving IUsesAnyService<MyService2>
            container.RegisterDecorator<UsesMyService2Decorator, IUsesAnyService<MyService2>>();

            // will be decorated twice
            var redecorated = container.Resolve<IUsesAnyService<MyService2>>();
            // but this will be decorated once
            var decorated = container.Resolve<IUsesAnyService<MyService1>>();

            Assert.IsType<UsesMyService2Decorator>(redecorated);
            Assert.IsType<UsesAnyServiceDecorator<MyService2>>(
                ((UsesMyService2Decorator)redecorated).Inner);

            // but the IUsesAnyService<MyService1> will only be decorated once
            Assert.IsType<UsesAnyServiceDecorator<MyService1>>(decorated);

            // </example5>
        }

        [Fact]
        public void ShouldInjectSpecialisedDecoratorBetweenTwoGenericDecorators()
        {
            // <example6>
            var container = new Container();
            container.RegisterType(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));
            container.RegisterDecorator(typeof(UsesAnyServiceDecorator<>), typeof(IUsesAnyService<>));
            //register the special decorator for IUsesAnyService<MyService2> again
            container.RegisterDecorator<UsesMyService2Decorator, IUsesAnyService<MyService2>>();
            container.RegisterDecorator(typeof(UsesAnyServiceDecorator2<>), typeof(IUsesAnyService<>));

            var reredecorated = Assert.IsType<UsesAnyServiceDecorator2<MyService2>>(
                container.Resolve<IUsesAnyService<MyService2>>());

            var redecorated = Assert.IsType<UsesMyService2Decorator>(reredecorated.Inner);
            var decorated = Assert.IsType<UsesAnyServiceDecorator<MyService2>>(redecorated.Inner);
            // </example6>
        }
    }
}
