using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Microsoft.Extensions.Hosting
{
    internal class RezolverServiceProviderFactory
        : IServiceProviderFactory<IRootTargetContainer>
    {
        public IRootTargetContainer CreateBuilder(IServiceCollection services)
        {
            var targetContainer = new TargetContainer();
            targetContainer.Populate(services);
            return targetContainer;
        }

        public IServiceProvider CreateServiceProvider(IRootTargetContainer containerBuilder)
        {
            return new ScopedContainer(containerBuilder);
        }
    }
}
