using Microsoft.Framework.DependencyInjection;
using System;
using System.Linq;
using Rezolver;
using System.Reflection;
using System.IO;

namespace Rezolver.Examples.AspNet5.Code
{
    public static class RezolverStartup
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

		public static IServiceProvider UseRezolver(IServiceCollection services)
		{
#if ASPNETCORE50
			var compiler = new RezolveTargetDelegateCompiler();
#else
			var compiler = new AssemblyRezolveTargetCompiler();
#endif
			var rezolver = new LoggingRezolver(compiler: compiler);
			rezolver.EventLogged += Rezolver_EventLogged;
			//code taken from 
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

		private static int _logCount = 0;

		private static void Rezolver_EventLogged(object sender, LoggingRezolver.LogEventArgs e)
		{
			using (var s = File.Open(@"C:\log.txt", _logCount++ == 0 ? FileMode.Truncate : FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				using (var w = new StreamWriter(s))
				{
					if(_logCount == 1)
					{
						w.WriteLine("Current registrations");

						foreach(var registration in ((IRezolver)sender).Builder.AllRegistrations)
						{
							w.WriteLine($"{registration.Key.RequestedType} {registration.Value.DeclaredType}");
                        }

						w.WriteLine();
					}

					w.WriteLine(e.Message);
				}
			}
		}

		private static IRezolveTarget CreateTargetFromService(IServiceDescriptor service, IRezolver rezolver)
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
				switch (service.Lifecycle)
				{
					case LifecycleKind.Singleton:
						target = new SingletonTarget(target);
						break;
					case LifecycleKind.Scoped:
						target = new ScopedSingletonTarget(target);
						break;
				}
			}

			return target;
		}
	}
}