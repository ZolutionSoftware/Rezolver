using Rezolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Framework.DependencyInjection.Rezolver
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public static class RezolverRegistration
    {
        private class RezolverScope : IServiceScope
        {
            public IServiceProvider ServiceProvider
            {
                get
                {
                    return _rezolver;
                }
            }

            public void Dispose()
            {
                _rezolver.Dispose();
            }

            private ILifetimeScopeRezolver _rezolver;

            public RezolverScope(ILifetimeScopeRezolver rezolver)
            {
                _rezolver = rezolver;
            }
        }
        private class RezolverScopeFactory : IServiceScopeFactory
        {
            private IRezolver _rezolver;
            public RezolverScopeFactory(IRezolver rezolver)
            {
                _rezolver = rezolver;
            }

            public IServiceScope CreateScope()
            {
                return new RezolverScope(_rezolver.CreateLifetimeScope());
            }
        }

        public static IRezolver CreateDefaultRezolver(IRezolverBuilder builder)
        {
#if DOTNET
			var compiler = new RezolveTargetDelegateCompiler();
#else
            var compiler = new AssemblyRezolveTargetCompiler();
#endif
            return new DefaultRezolver(builder, compiler);
        }

        public static IServiceProvider Populate(this IRezolver rezolver, IServiceCollection services)
        {
            if (rezolver == null) throw new ArgumentNullException(nameof(rezolver));

            //register service provider
            rezolver.RegisterExpression(context => context.Rezolver, typeof(IServiceProvider));
            //register scope factory - uses the rezolver that comes in the context.
            rezolver.RegisterExpression(context => new RezolverScopeFactory(context.Rezolver), typeof(IServiceScopeFactory));

            foreach (var group in services.GroupBy(s => s.ServiceType))
            {
                var toRegister = group.Select(s => CreateTargetFromService(s, rezolver)).Where(t => t != null).ToArray();

                if (toRegister.Length == 1)
                    rezolver.Register(toRegister[0], group.Key);
                else if (toRegister.Length > 1)
                    rezolver.RegisterMultiple(toRegister, group.Key);
            }

            return rezolver;
        }

        private static IRezolveTarget CreateTargetFromService(ServiceDescriptor service, IRezolver rezolver)
        {
            IRezolveTarget target = null;
            //three main types of service registration - delegate factory, 
            if (service.ImplementationType != null)
            {
                if (service.ImplementationType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    target = GenericConstructorTarget.Auto(service.ImplementationType);
                }
                else
                {
                    target = ConstructorTarget.Auto(service.ImplementationType);
                }
            }
            else if (service.ImplementationInstance != null)
            {
                target = new ObjectTarget(service.ImplementationInstance, service.ServiceType);
            }
            else if (service.ImplementationFactory != null)
            {
                //not ideal - need ability to provide a delegate that accepts a rezolve context
                //as a parameter that can then be fed on to the delegate, that way we can ensure that
                //any scoping is honoured.
                target = new DelegateTarget<object>(() => service.ImplementationFactory(rezolver), service.ServiceType);
            }

            if (target != null)
            {
                switch (service.Lifetime)
                {
                    case ServiceLifetime.Singleton:
                        target = new SingletonTarget(target);
                        break;
                    case ServiceLifetime.Scoped:
                        target = new ScopedTarget(target);
                        break;
                }
            }

            return target;

            /*
            namespace Microsoft.Framework.DependencyInjection.Tests
{
    public class RezolverContainerTests : ScopingContainerTestBase
    {
        protected override IServiceProvider CreateContainer()
        {
            var rezolver = new DefaultLifetimeScopeRezolver();

            rezolver.Populate(TestServices.DefaultServices());

            return rezolver.Resolve<IServiceProvider>();
        }
    }
}

            */
        }
    }
}
