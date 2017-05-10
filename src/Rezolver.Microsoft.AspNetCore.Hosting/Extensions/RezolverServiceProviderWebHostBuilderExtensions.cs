// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using Rezolver;
using System;

namespace Microsoft.AspNetCore.Hosting
{
	/// <summary>
	/// Provides core functionality to web applications using Asp.Net Core 1.1 and above
	/// to inject Rezolver containers into the application at the web hosting level, rather 
	/// than while the application's startup phase is executing.
	/// </summary>
	public static class RezolverServiceProviderWebHostBuilderExtensions
	{ 
		/// <summary>
		/// Configures the builder so that the standard Rezolver container will be used as the
		/// DI for the web host produced by the builder.
		/// </summary>
		/// <param name="builder">The builder.</param>
		public static IWebHostBuilder UseRezolver(this IWebHostBuilder builder/*, Action<LoggingCallTrackerOptions> configureCallTrackingOptionsCallback = null*/)
		{
			return builder.ConfigureServices(services => services.AddRezolver(/*configureCallTrackingOptionsCallback*/));
		}
	}
}
