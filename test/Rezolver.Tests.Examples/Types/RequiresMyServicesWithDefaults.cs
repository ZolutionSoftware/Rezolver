using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class RequiresMyServicesWithDefaults : RequiresMyServices
	{
		public RequiresMyServicesWithDefaults(MyService1 service1)
			: base(service1)
		{

		}

		public RequiresMyServicesWithDefaults(MyService1 service1,
			MyService2 service2 = null,
			MyService3 service3 = null)
			: base(service1,
				  service2 ?? Default2,
				  service3 ?? Default3)
		{

		}
	}
	//</example>
}
