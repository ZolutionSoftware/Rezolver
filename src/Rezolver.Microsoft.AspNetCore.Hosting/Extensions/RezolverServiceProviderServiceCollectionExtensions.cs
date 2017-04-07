// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
        /// Adds a Rezolver service provider factory to the service collection.  Used by the
        /// <see cref="Microsoft.AspNetCore.Hosting.RezolverServiceProviderWebHostBuilderExtensions.UseRezolver(AspNetCore.Hosting.IWebHostBuilder)"/>
        /// extension method.
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
