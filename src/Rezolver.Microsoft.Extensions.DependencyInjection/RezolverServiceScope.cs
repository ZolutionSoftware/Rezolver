using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Microsoft.Extensions.DependencyInjection
{
	internal class RezolverServiceScope : IServiceScope
	{
		public IServiceProvider ServiceProvider
		{
			get
			{
				return _container;
			}
		}

		public void Dispose()
		{
			_container.Dispose();
		}

		private IScopedContainer _container;

		public RezolverServiceScope(IScopedContainer container)
		{
			_container = container;
		}
	}
}
