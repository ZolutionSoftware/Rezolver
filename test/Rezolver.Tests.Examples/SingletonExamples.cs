using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class SingletonExamples
    {
        [Fact]
        public void ShouldCreateOneIMyService()
        {
            // <example1>
            var container = new Container();
            // The RegisterSingleton overload is like the RegisterType
            // overload, it creates a ConstructorTarget/GenericConstructorTarget
            // and wraps it in a SingletonTarget
            container.RegisterSingleton<MyService, IMyService>();

            var result1 = container.Resolve<IMyService>();
            var result2 = container.Resolve<IMyService>();

            Assert.Same(result1, result2);
            // </example1>
        }

        [Fact]
        public void ShouldCreateOneIMyService_ITarget()
        {
            // <example2>
            // This time, because we're binding to a specific constructor with named
            // arguments, we have to create a ConstructorTarget manually and then 
            // convert it to a Singleton with the .Singleton() extension method
            var container = new Container();

            container.RegisterType<MyService, IMyService>();
            container.Register(
                Target.ForType<RequiresIMyServiceAndDateTime>(new {
                    startDate = Target.ForObject(DateTime.UtcNow.AddDays(1))
                }
            ).Singleton()); // //<-- Singleton created here

            var result1 = container.Resolve<RequiresIMyServiceAndDateTime>();
            var result2 = container.Resolve<RequiresIMyServiceAndDateTime>();

            Assert.Same(result1, result2);
            // </example2>
        }

        [Fact]
        public void ShouldCreateUniqueSingletonForEachConcreteGeneric()
        {
            // <example3>
            var container = new Container();
            // note - UsesAnyService<> doesn't have any dependencies
            container.RegisterSingleton(typeof(UsesAnyService<>), typeof(IUsesAnyService<>));

            var result1a = container.Resolve<IUsesAnyService<MyService1>>();
            var result1b = container.Resolve<IUsesAnyService<MyService1>>();
            var result2a = container.Resolve<IUsesAnyService<MyService2>>();
            var result2b = container.Resolve<IUsesAnyService<MyService2>>();

            Assert.Same(result1a, result1b);
            Assert.Same(result2a, result2b);
            // </example3>
        }

        [Fact]
        public void ShouldExecuteFactoryDelegateOnlyOnce()
        {
            // <example4>
            var container = new Container();

            // Incremented by the delegate registered below
            int counter = 0;

            container.Register(
                Target.ForDelegate(() => ++counter).Singleton()
            );

            var result1 = container.Resolve<int>();
            var result2 = container.Resolve<int>();

            Assert.Equal(counter, result1);
            Assert.Equal(result1, result2);
            // </example4>
        }

        [Fact]
        public void ShouldExecuteExpressionOnlyOnce()
        {
            // <example5>
            // Similar to the delegate example above,
            // but we're injecting an instance which holds the counter
            // so we can use a PreIncrementAssign UnaryExpression, which you 
            // can't do in compiler-built lambda expressions.
            // In any case, you can't change the value of a lifted local in 
            // an expression, because it's lifted as a constant.
            var container = new Container();
            
            container.RegisterSingleton<CounterHolder>();

            // expression below is equivalent to:
            // (CounterHolder c) => ++c.Counter
            var counterHolderParam = Expression.Parameter(typeof(CounterHolder));
            container.Register(
                new ExpressionTarget(Expression.Lambda(
                    Expression.PreIncrementAssign(
                        Expression.Property(counterHolderParam, "Counter")
                    ),
                    counterHolderParam
                )).Singleton()
            );

            // get the singleton CounterHolder and change its counter to 10
            var counterHolder = container.Resolve<CounterHolder>();
            counterHolder.Counter = 10;

            // now resolve two ints via the expression which would, if 
            // the expression wasn't registered as a singleton, increment
            // the counterHolder.Counter property twice
            var result1 = container.Resolve<int>();
            var result2 = container.Resolve<int>();

            // counterHolder's Counter should have been incremented only once
            Assert.Equal(counterHolder.Counter, result1);
            Assert.Equal(result1, result2);
            // </example5>
        }

        [Fact]
        public void ShouldInjectSingletonDependenciesIntoTransient()
        {
            // <example6>
            var container = new Container();
            container.RegisterType<RequiresMyServices>();
            container.RegisterSingleton<MyService1>();
            container.RegisterSingleton<MyService2>();
            container.RegisterSingleton<MyService3>();

            var service1 = container.Resolve<MyService1>();
            var service2 = container.Resolve<MyService2>();
            var service3 = container.Resolve<MyService3>();
            var dependant1 = container.Resolve<RequiresMyServices>();
            var dependant2 = container.Resolve<RequiresMyServices>();

            Assert.NotSame(dependant1, dependant2);
            Assert.Same(service1, dependant1.Service1);
            Assert.Same(service2, dependant1.Service2);
            Assert.Same(service3, dependant1.Service3);
            Assert.Same(dependant1.Service1, dependant2.Service1);
            Assert.Same(dependant1.Service2, dependant2.Service2);
            Assert.Same(dependant1.Service3, dependant2.Service3);
            // </example6>
        }

        [Fact]
        public void ShouldInjectTransientDependenciesIntoSingleton()
        {
            // <example7>
            var container = new Container();
            container.RegisterSingleton<RequiresMyServices>();
            container.RegisterType<MyService1>();
            container.RegisterType<MyService2>();
            container.RegisterType<MyService3>();

            var service1 = container.Resolve<MyService1>();
            var service2 = container.Resolve<MyService2>();
            var service3 = container.Resolve<MyService3>();
            var dependant1 = container.Resolve<RequiresMyServices>();
            var dependant2 = container.Resolve<RequiresMyServices>();

            Assert.Same(dependant1, dependant2);
            Assert.NotSame(service1, dependant1.Service1);
            Assert.NotSame(service2, dependant1.Service2);
            Assert.NotSame(service3, dependant1.Service3);
            // </example7>
        }

        [Fact]
        public void ShouldInjectSingletonsIntoEnumerable()
        {
            // <example8>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterSingleton<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            var result1 = container.Resolve<IEnumerable<IMyService>>().ToArray();
            var result2 = container.Resolve<IEnumerable<IMyService>>().ToArray();

            Assert.NotSame(result1[0], result2[0]);
            Assert.Same(result1[1], result2[1]);
            Assert.NotSame(result1[2], result2[2]);
            // </example8>
        }

        [Fact]
        public void ShouldDecorateSingleton()
        {
            // <example9>
            // See the notes on the decorators topic for why we have to
            // create a TargetContainer for registrations in this example.
            var targets = new TargetContainer();
            targets.RegisterSingleton<MyService1, IMyService>();
            targets.RegisterDecorator<MyServiceDecorator1, IMyService>();

            var container = new Container(targets);
            var decorator1 = 
                Assert.IsType<MyServiceDecorator1>(container.Resolve<IMyService>());
            var decorator2 =
                Assert.IsType<MyServiceDecorator1>(container.Resolve<IMyService>());

            Assert.Same(decorator1.Inner, decorator2.Inner);
            // </example9>
        }
    }
}
