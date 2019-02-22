using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Microsoft.Extensions.Hosting
{
    /// <summary>
    /// For the generic host
    /// </summary>
    internal class NetCoreServiceProviderFactory
        : IServiceProviderFactory<IRootTargetContainer>
    {
        private readonly Action<RezolverOptions> _configureOptions;
        private readonly RezolverOptions _options = new RezolverOptions();

        internal NetCoreServiceProviderFactory(Action<RezolverOptions> configureOptions = null)
        {
            _configureOptions = configureOptions;
        }

        public IRootTargetContainer CreateBuilder(IServiceCollection services)
        {
            _configureOptions?.Invoke(_options);

            var targetContainer = new TargetContainer(_options.TargetContainerConfig ?? TargetContainer.DefaultConfig.Clone());

            targetContainer.Populate(services);
            return targetContainer;
        }

        public IServiceProvider CreateServiceProvider(IRootTargetContainer containerBuilder)
        {
            return new ScopedContainer(containerBuilder, _options.ContainerConfig ?? Container.DefaultConfig.Clone());
        }
    }
}
