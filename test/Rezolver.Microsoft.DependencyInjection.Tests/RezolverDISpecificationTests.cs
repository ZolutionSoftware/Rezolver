using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;
using Rezolver.Compilation.Expressions;

namespace Rezolver.Microsoft.Extensions.DependencyInjection.Tests
{
    public class RezolverDISpecificationTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            //Just proving my comments on https://github.com/aspnet/DependencyInjection/issues/589
            var config = TargetContainer
                .DefaultConfig
                .Clone()
                .ConfigureOption<Options.LazyEnumerables>(false);

            var container = new ScopedContainer(
                new TargetContainer(config));

            container.Populate(serviceCollection);
            return container;
        }
    }
}
