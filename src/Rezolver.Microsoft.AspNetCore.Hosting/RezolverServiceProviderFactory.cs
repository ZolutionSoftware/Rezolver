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
    /// Providing a more flexible way to configure your web application to use Rezolver.
    /// </summary>
	internal class RezolverServiceProviderFactory : IServiceProviderFactory<ITargetContainer>
	{
		private RezolverOptions _options;

        /// <summary>
        /// Creates a new instance of the 
        /// </summary>
        /// <param name="options"></param>
		public RezolverServiceProviderFactory(IOptions<RezolverOptions> options) 
            => _options = options.Value;

        public ITargetContainer CreateBuilder(IServiceCollection services)
		{
			var toReturn = CreateTargetContainer(services);
			toReturn.Populate(services);
			return toReturn;
		}
		
		public IServiceProvider CreateServiceProvider(ITargetContainer containerBuilder) 
            => CreateDefaultContainer(containerBuilder);

		protected ITargetContainer CreateTargetContainer(IServiceCollection services) 
            => new TargetContainer(_options.TargetContainerConfig);

		protected IContainer CreateDefaultContainer(ITargetContainer targetContainer)
            => new ScopedContainer(targetContainer, _options.ContainerConfig);
	}
}
