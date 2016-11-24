// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using Rezolver;
using Rezolver.Logging;
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
		/// <param name="targetContainer">Optional.  A target container which will store all service
		/// registrations.  If left as <c>null</c> then a new instance of <see cref="TargetContainer" /> will
		/// be used.</param>
		/// <param name="containerFactoryOverride">Optional.  Delegate to be called when a new <see cref="IContainer"/>
		/// is required.  If left as <c>null</c>, then the default behaviour is to pass the <see cref="ITargetContainer"/>
		/// to a new instance of <see cref="ScopedContainer"/>.
		/// </param>
		public static IWebHostBuilder UseRezolver(this IWebHostBuilder builder/*, Action<LoggingCallTrackerOptions> configureCallTrackingOptionsCallback = null*/)
		{
			return builder.ConfigureServices(services => services.AddRezolver(/*configureCallTrackingOptionsCallback*/));
		}
	}
}
