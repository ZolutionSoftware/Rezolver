// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using Rezolver;
using System;

namespace Microsoft.AspNetCore.Hosting
{
	/// <summary>
	/// Provides core functionality to web applications using Asp.Net Core 
	/// to inject Rezolver containers into the application at the web hosting level, rather 
	/// than while the application's startup phase is executing.
	/// </summary>
	public static class RezolverServiceProviderWebHostBuilderExtensions
	{
        /// <summary>
        /// Configures the builder so that the standard Rezolver container will be used as the
        /// DI for the web host produced by the builder, with an optional configuration callback
        /// for a <see cref="RezolverOptions"/> instance that is used to customise the behaviour
        /// of the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configureRezolverOptions">Optional. Configuration callback to be applied to the <see cref="RezolverOptions"/>
        /// instance that configures the <see cref="ITargetContainer"/> and <see cref="IContainer"/> creation process.
        /// 
        /// Note - in order for this to be applied, your application will also need to call the 
        /// <see cref="OptionsServiceCollectionExtensions.AddOptions(IServiceCollection)"/> extension method (usually in your 
        /// application's ConfigureServices method).</param>
        public static IWebHostBuilder UseRezolver(this IWebHostBuilder builder, Action<RezolverOptions> configureRezolverOptions = null)
		{
			return builder.ConfigureServices(services => services.AddRezolver(configureRezolverOptions));
		}
	}
}
