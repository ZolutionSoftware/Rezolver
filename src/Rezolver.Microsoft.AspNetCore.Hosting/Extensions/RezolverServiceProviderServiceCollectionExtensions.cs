// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides the <see cref="AddRezolver(IServiceCollection)"/> extension method which adds the 
    /// <c>IServiceProviderFactory</c> service registration for the <see cref="ITargetContainer"/> service provider builder.
    /// </summary>
    public static class RezolverServiceProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a Rezolver service provider factory to the service collection.  Used by the
        /// <see cref="Microsoft.AspNetCore.Hosting.RezolverServiceProviderWebHostBuilderExtensions.UseRezolver(AspNetCore.Hosting.IWebHostBuilder)"/>
        /// extension method.
        /// </summary>
        /// <param name="services">The services to be configured.</param>
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
