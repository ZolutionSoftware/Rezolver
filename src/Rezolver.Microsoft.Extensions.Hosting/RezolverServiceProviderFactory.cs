// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rezolver
{
    /// <summary>
    /// For the generic host
    /// </summary>
    public class RezolverServiceProviderFactory
        : IServiceProviderFactory<IRootTargetContainer>
    {
        private readonly Action<RezolverOptions> _configureOptions;
        private readonly RezolverOptions _options = new RezolverOptions();

        /// <summary>
        /// Initialises a new instance of the <see cref="RezolverServiceProviderFactory"/>
        /// with an optional callback for configuring the factory options.
        /// </summary>
        /// <param name="configureOptions"></param>
        public RezolverServiceProviderFactory(Action<RezolverOptions> configureOptions = null)
        {
            _configureOptions = configureOptions;
        }

        /// <summary>
        /// Implementation of <see cref="IServiceProviderFactory{TContainerBuilder}.CreateBuilder(IServiceCollection)"/>
        /// </summary>
        /// <param name="services">The service collection to populate the container builder with</param>
        /// <returns>The container builder (<see cref="IRootTargetContainer"/>)</returns>
        public IRootTargetContainer CreateBuilder(IServiceCollection services)
        {
            _configureOptions?.Invoke(_options);

            var targetContainer = new TargetContainer(_options.TargetContainerConfig ?? TargetContainer.DefaultConfig.Clone());

            targetContainer.Populate(services);
            return targetContainer;
        }

        /// <summary>
        /// Implementation of <see cref="IServiceProviderFactory{TContainerBuilder}.CreateServiceProvider(TContainerBuilder)"/>
        /// </summary>
        /// <param name="containerBuilder">The root target container to use to construct the underlying rezolver container</param>
        /// <returns>Returns a reference to the <see cref="IServiceProvider"/> implementation of <see cref="ScopedContainer"/> </returns>
        public IServiceProvider CreateServiceProvider(IRootTargetContainer containerBuilder)
        {
            return new ScopedContainer(containerBuilder, _options.ContainerConfig ?? Container.DefaultConfig.Clone());
        }
    }
}
