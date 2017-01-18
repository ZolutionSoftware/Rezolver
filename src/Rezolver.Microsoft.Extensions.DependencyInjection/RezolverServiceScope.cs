// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
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
