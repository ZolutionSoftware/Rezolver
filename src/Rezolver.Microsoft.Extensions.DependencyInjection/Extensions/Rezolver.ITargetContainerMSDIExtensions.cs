using Microsoft.Extensions.DependencyInjection;
using Rezolver.Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
	public static class ITargetContainerMSDIExtensions
	{
		public static void Populate(this ITargetContainer targets, IServiceCollection services)
		{
			if (targets == null) throw new ArgumentNullException(nameof(targets));
			//register service provider
			targets.RegisterExpression(context => context.Container, typeof(IServiceProvider));
			//register scope factory - uses the rezolver that comes in the context.
			targets.RegisterExpression(context => new RezolverContainerScopeFactory(context.Container), typeof(IServiceScopeFactory));

			foreach (var group in services.GroupBy(s => s.ServiceType))
			{
				var toRegister = group.Select(s => CreateTargetFromService(s)).Where(t => t != null).ToArray();

				if (toRegister.Length == 1)
					targets.Register(toRegister[0], group.Key);
				else if (toRegister.Length > 1)
					targets.RegisterMultiple(toRegister, group.Key);
			}
		}

		private static ITarget CreateTargetFromService(ServiceDescriptor service)
		{
			ITarget target = null;
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
				target = new DelegateTarget<object>(c => service.ImplementationFactory(c.Container), service.ServiceType);
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
		}
	}
}
