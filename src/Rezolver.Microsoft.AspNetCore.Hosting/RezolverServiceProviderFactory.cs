// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rezolver.Logging;
using Rezolver.Compilation;
using System;

namespace Rezolver
{
	internal class RezolverServiceProviderFactory : IServiceProviderFactory<ITargetContainer>
	{
		private RezolverOptions _options;

		public RezolverServiceProviderFactory(IOptions<RezolverOptions> options)
		{
			_options = options.Value;
		}

		public ITargetContainer CreateBuilder(IServiceCollection services)
		{
			var toReturn = CreateDefaultTargetContainer(services);
			toReturn.Populate(services);
			return toReturn;
		}
		
		public IServiceProvider CreateServiceProvider(ITargetContainer containerBuilder)
		{
			return CreateDefaultContainer(containerBuilder);
		}

		protected virtual ITargetContainer CreateDefaultTargetContainer(IServiceCollection services)
		{
			if (_options.Tracking.CallTracker != null)
				return new TrackedTargetContainer(_options.Tracking.CallTracker);
			else
				return new TargetContainer();
		}

		protected virtual IContainer CreateDefaultContainer(ITargetContainer targetContainer)
		{
			if (_options.Tracking.CallTracker != null)
				throw new NotImplementedException("Scoping has been changed and the call tracking stuff currently doesn't work - it will");
				//return new TrackedContainerScope(_options.Tracking.CallTracker, new ContainerScope(new Container(targetContainer)));
			else
				return new ScopedContainer(targetContainer);
		}
	}
}
