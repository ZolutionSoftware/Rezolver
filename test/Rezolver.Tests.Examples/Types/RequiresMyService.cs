using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class RequiresMyService : IRequiresIMyService
	{
		public MyService Service { get; }
		public RequiresMyService(MyService service)
		{
			Service = service;
		}

		public RequiresMyService(IMyService service)
		{
			if (service.GetType() != typeof(MyService))
			{
				throw new ArgumentException($"{ service.GetType() } not supported",
					nameof(service));
			}
			Service = (MyService)service;
		}

		IMyService IRequiresIMyService.Service { get { return Service; } }
	}
	//</example>
}
