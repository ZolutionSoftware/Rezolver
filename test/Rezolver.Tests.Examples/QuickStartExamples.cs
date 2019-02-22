using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class QuickStartExamples
    {
        // <mytype>
        internal class MyType
        {

        }
        // </mytype>

        // <requiresmytype>
        internal class RequiresMyType
        {
            public MyType Dependency { get; }
            public RequiresMyType(MyType dependency)
            {
                Dependency = dependency;
            }
        }
        // </requiresmytype>

        // <iabstraction>
        internal interface IAbstraction
        {
            void DoSomething();
        }

        internal class Implementation : IAbstraction
        {
            public Implementation()
            {

            }

            public void DoSomething()
            {
                throw new NotImplementedException();
            }
        }

        internal class RequiresAbstraction
        {
            public IAbstraction Dependency { get; }

            public RequiresAbstraction(IAbstraction abstraction)
            {
                Dependency = abstraction;
            }
        }
        // </iabstraction>

        [Fact]
        public void RegisterAndResolve()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<MyType>();

            MyType instance = container.Resolve<MyType>();
            // </example1>

            Assert.NotNull(instance);
        }

        [Fact]
        public void RegisterAlternatives()
        {
            var container = new Container();

            // <example2>
            container.RegisterType(typeof(MyType));
            container.Register(Target.ForType<MyType>());
            container.Register(Target.ForType(typeof(MyType)));
            // </example2>

            Assert.Equal(3, container.Resolve<MyType[]>().Length);
        }

        [Fact]
        public void ResolveAlternatives()
        {
            var container = new Container();

            container.RegisterType<MyType>();

            // <example3>
            MyType instance = (MyType)container.Resolve(typeof(MyType));
            MyType instance2 = (MyType)container.Resolve(new ResolveContext(container, typeof(MyType)));
            // </example3>

            Assert.NotNull(instance);
            Assert.NotNull(instance2);
            Assert.NotSame(instance, instance2);
        }

        [Fact]
        public void ConstructorDependencies()
        {
            var container = new Container();

            // <example4>
            container.RegisterType<MyType>();
            container.RegisterType<RequiresMyType>();
            var instance = container.Resolve<RequiresMyType>();
            // </example4>

            Assert.NotNull(instance);
            Assert.NotNull(instance.Dependency);
        }

        [Fact]
        public void InjectingAnAbstraction()
        {
            var container = new Container();

            // <example10>
            container.RegisterType<Implementation, IAbstraction>();
            container.RegisterType<RequiresAbstraction>();
            var instance = container.Resolve<RequiresAbstraction>();
            // </example10>

            Assert.NotNull(instance);
            Assert.NotNull(instance.Dependency);
        }

        [Fact]
        public void MSDIExample()
        {
            // <example20>
            var container = new Container();
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<MyType>();
            services.AddTransient<RequiresMyType>();
            services.AddTransient<IAbstraction, Implementation>();
            services.AddTransient<RequiresAbstraction>();

            container.Populate(services);

            var instance1 = container.Resolve<RequiresMyType>();
            var instance2 = container.Resolve<RequiresAbstraction>();
            // </example20>

            Assert.NotNull(instance1);
            Assert.NotNull(instance1.Dependency);
            Assert.NotNull(instance2);
            Assert.NotNull(instance2.Dependency);
        }

        [Fact]
        public void MSDIExample2()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<MyType>();
            services.AddTransient<RequiresMyType>();
            services.AddTransient<IAbstraction, Implementation>();
            services.AddTransient<RequiresAbstraction>();

            // <example21>
            var container = services.CreateRezolverContainer();

            var instance1 = container.Resolve<RequiresMyType>();
            var instance2 = container.Resolve<RequiresAbstraction>();
            // </example21>

            Assert.NotNull(instance1);
            Assert.NotNull(instance1.Dependency);
            Assert.NotNull(instance2);
            Assert.NotNull(instance2.Dependency);
        }
    }
}
