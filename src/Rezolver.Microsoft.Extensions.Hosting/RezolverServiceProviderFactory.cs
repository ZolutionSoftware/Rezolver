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
        private readonly Action<RezolverOptions, IRootTargetContainer> _configureContainerBeforePopulate;
        private readonly Action<RezolverOptions, IRootTargetContainer> _configureContainerAfterPopulate;
        private readonly RezolverOptions _options = new RezolverOptions();

        /// <summary>
        /// Initialises a new instance of the <see cref="RezolverServiceProviderFactory"/>
        /// with optional callbacks for configuring the factory options, and the <see cref="IRootTargetContainer"/>
        /// before and after it's been populated with services from the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="configureOptions">An optional delegate which customises the options that are passed to the
        /// <see cref="TargetContainer"/> and, later, the <see cref="ScopedContainer"/> on construction.</param>
        /// <param name="configureContainerBeforePopulate">An optional delegate that can be used to perform registrations
        /// and/or configuration of the <see cref="IRootTargetContainer"/> that is built by the <see cref="CreateBuilder(IServiceCollection)"/>
        /// implementation, and *before* the services are populated from the service collection.</param>
        /// <param name="configureContainerAfterPopulate">Similar to <paramref name="configureContainerBeforePopulate"/>, except this delegate
        /// will be called after the services have been populated from the service collection.</param>
        public RezolverServiceProviderFactory(Action<RezolverOptions> configureOptions = null,
            Action<RezolverOptions, IRootTargetContainer> configureContainerBeforePopulate = null,
            Action<RezolverOptions, IRootTargetContainer> configureContainerAfterPopulate = null)
        {
            _configureOptions = configureOptions;
            _configureContainerBeforePopulate = configureContainerBeforePopulate;
            _configureContainerAfterPopulate = configureContainerAfterPopulate;
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

            _configureContainerBeforePopulate?.Invoke(_options, targetContainer);
            targetContainer.Populate(services);
            _configureContainerAfterPopulate?.Invoke(_options, targetContainer);

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
