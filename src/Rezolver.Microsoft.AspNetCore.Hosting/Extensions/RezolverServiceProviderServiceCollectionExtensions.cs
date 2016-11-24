using Rezolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RezolverServiceProviderServiceCollectionExtensions
    {
		/// <summary>
		/// Adds a Rezolver service provider factory to the service collection, with the option to pass
		/// an <see cref="ITargetContainer" /> that is already created for storing the targets for the
		/// <see cref="IContainer" /> that will ultimately be constructed.
		/// The creation of the container itself can also be overriden through the
		/// <paramref name="containerFactoryOverride" /> parameter.
		/// </summary>
		/// <param name="services">The services to be configured.</param>
		/// <param name="targetContainer">Optional.  A target container which will store all service
		/// registrations.  If left as <c>null</c> then a new instance of <see cref="TargetContainer" /> will
		/// be used.</param>
		/// <param name="containerFactoryOverride">Optional.  Delegate to be called when a new <see cref="IContainer"/>
		/// is required.  If left as <c>null</c>, then the default behaviour is to pass the <see cref="ITargetContainer"/>
		/// to a new instance of <see cref="ScopedContainer"/>.
		/// </param>
		public static IServiceCollection AddRezolver(this IServiceCollection services/*, Action<LoggingCallTrackerOptions> configureCallTrackingOptionsCallback = null*/)
		{

			//if(configureCallTrackingOptionsCallback != null)
			//{
			//	//call tracking is being configured, so register the call tracker and the configuration callback
			//	services.Configure(configureCallTrackingOptionsCallback);
			//	services.AddSingleton<ICallTracker, LoggingCallTracker>();
			//}
			return services.AddSingleton<IServiceProviderFactory<ITargetContainer>, RezolverServiceProviderFactory>();
		}
	}
}
