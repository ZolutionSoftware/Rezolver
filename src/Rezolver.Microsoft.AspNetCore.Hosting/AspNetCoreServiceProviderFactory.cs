// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rezolver.Compilation;
using System;

namespace Rezolver
{
    /// <summary>
    /// Implementation of the <see cref="IServiceProviderFactory{TContainerBuilder}"/> interface.
    /// Providing a more flexible way to configure an Asp.Net Core application to use Rezolver.
    /// </summary>
	internal class AspNetCoreServiceProviderFactory 
        : IServiceProviderFactory<IRootTargetContainer>, IServiceProviderFactory<ITargetContainer>
	{
		private RezolverOptions _options;

        /// <summary>
        /// Creates a new instance of the 
        /// </summary>
        /// <param name="options"></param>
		public AspNetCoreServiceProviderFactory(IOptions<RezolverOptions> options) 
            => _options = options.Value;

        public IRootTargetContainer CreateBuilder(IServiceCollection services)
		{
			var toReturn = CreateTargetContainer(services);
			toReturn.Populate(services);
			return toReturn;
		}
		
		public IServiceProvider CreateServiceProvider(IRootTargetContainer targets) 
            => CreateDefaultContainer(targets);

		protected IRootTargetContainer CreateTargetContainer(IServiceCollection services) 
            => new TargetContainer(_options.TargetContainerConfig);

		protected IContainer CreateDefaultContainer(IRootTargetContainer targets)
            => new ScopedContainer(targets, _options.ContainerConfig);

        ITargetContainer IServiceProviderFactory<ITargetContainer>.CreateBuilder(IServiceCollection services)
        {
            return CreateBuilder(services);
        }

        public IServiceProvider CreateServiceProvider(ITargetContainer containerBuilder)
        {
            if (!(containerBuilder is IRootTargetContainer rootTargets))
                throw new ArgumentException($"ITargetContainer of type { containerBuilder.GetType() } is not supported.  Type must implement IRootTargetContainer", nameof(containerBuilder));
            return CreateServiceProvider(rootTargets);
        }
    }
}
