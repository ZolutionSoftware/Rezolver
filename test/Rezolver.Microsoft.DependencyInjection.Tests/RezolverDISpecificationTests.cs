using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Rezolver.Microsoft.Extensions.DependencyInjection.Tests
{
	// This project can output the Class library as a NuGet Package.
	// To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
	public class RezolverDISpecificationTests : DependencyInjectionSpecificationTests
	{
		protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
		{
			return new ScopedContainer().Populate(serviceCollection);
		}
	}
}
