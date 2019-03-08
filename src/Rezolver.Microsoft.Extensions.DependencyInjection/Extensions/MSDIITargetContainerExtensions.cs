// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using Rezolver.Targets;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
    /// <summary>
    /// Provides the <see cref="Populate(ITargetContainer, IServiceCollection)"/> extension method which allows
    /// easy importing of all registrations in an <see cref="IServiceCollection"/> from <c>Microsoft.Extensions.DependencyInjection</c>
    /// into a Rezolver <see cref="ITargetContainer"/>.
    /// </summary>
	public static class MSDIITargetContainerExtensions
	{
        /// <summary>
        /// Translates all registrations in <paramref name="services"/> into registered targets in <paramref name="targets"/>.
        /// </summary>
        /// <param name="targets">The target container into which the registrations will be added</param>
        /// <param name="services">The service collection to be registered</param>
		public static void Populate(this ITargetContainer targets, IServiceCollection services)
		{
			if (targets == null) throw new ArgumentNullException(nameof(targets));
            //register service provider - ensuring that it's marked as unscoped because the lifetimes of
            //containers which are also scopes are managed by the code that creates them, not by the containers themselves
            targets.Register(Target.ForExpression(context => (IServiceProvider)context).Unscoped());
            //register scope factory - uses the context as a scope factory (it will choose between the container or	
            //the scope as the actual scope factory that will be used.	
            targets.RegisterExpression(context => new RezolverContainerScopeFactory(context), typeof(IServiceScopeFactory));

            foreach (var serviceAndTarget in services.Select(s => new {
                serviceType = s.ServiceType,
                target = CreateTargetFromService(s)
            }).Where(st => st.target != null))
            {
                targets.Register(serviceAndTarget.target, serviceAndTarget.serviceType);
            }
		}

		private static ITarget CreateTargetFromService(ServiceDescriptor service)
		{
			ITarget target = null;
			//three main types of service registration - delegate factory, 
			if (service.ImplementationType != null)
			{
                //this factory function checks for Generic Types and uses GenericConstructorTarget accordingly.
				target = Target.ForType(service.ImplementationType);
			}
			else if (service.ImplementationInstance != null)
			{
				target = new ObjectTarget(service.ImplementationInstance, service.ServiceType);
			}
			else if (service.ImplementationFactory != null)
			{
				target = Target.ForDelegate(c => service.ImplementationFactory(c), service.ServiceType);
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
