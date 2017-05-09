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
      var container = new ScopedContainer();
      container.Populate(serviceCollection);
      return container;
    }
  }
}
