// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.AspNetCore.Routing;
using Rezolver;
using Rezolver.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides the <see cref="AddRezolver(IServiceCollection, Action{RezolverOptions})"/> extension method which adds the 
    /// <c>IServiceProviderFactory</c> service registration for the <see cref="ITargetContainer"/> service provider builder.
    /// </summary>
    public static class RezolverServiceProviderServiceCollectionExtensions
    {
        private static void ConfigureDefaultOptions(IServiceCollection services)
        {
            services.Configure<RezolverOptions>(o =>
            {
                // runtime bug only seen in Release mode - recursive singleton when UseMvc is called
                o.TargetContainerConfig.ConfigureOption<EnableEnumerableCovariance, EndpointDataSource>(false);
            });
        }
        /// <summary>
        /// Adds a Rezolver service provider factory to the service collection.  Used by the
        /// <see cref="Microsoft.AspNetCore.Hosting.RezolverServiceProviderWebHostBuilderExtensions.UseRezolver(AspNetCore.Hosting.IWebHostBuilder, Action{RezolverOptions})"/>
        /// extension method.
        /// </summary>
        /// <param name="services">Required. The services to be configured.</param>
        /// <param name="configureRezolverOptions">Optional. Configuration callback to be applied to the <see cref="RezolverOptions"/>
        /// instance that configures the <see cref="ITargetContainer"/> and <see cref="IContainer"/> creation process.
        /// 
        /// Note - in order for this to be applied, your application will also need to call the 
        /// <see cref="OptionsServiceCollectionExtensions.AddOptions(IServiceCollection)"/> extension method (usually in your 
        /// application's ConfigureServices method).</param>
        public static IServiceCollection AddRezolver(this IServiceCollection services, Action<RezolverOptions> configureRezolverOptions = null)
		{
            ConfigureDefaultOptions(services);

            if (configureRezolverOptions != null)
                services.Configure(configureRezolverOptions);

			return services.AddSingleton<IServiceProviderFactory<IRootTargetContainer>, AspNetCoreServiceProviderFactory>()
                .AddSingleton<IServiceProviderFactory<ITargetContainer>, AspNetCoreServiceProviderFactory>();
		}
	}
}
