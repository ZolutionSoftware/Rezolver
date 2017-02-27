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
			MyService3 service3 = null,
			MyService4 service4 = null,
			MyService5 service5 = null,
			MyService6 service6 = null)
			: base(service1,
				  service2 ?? Default2,
				  service3 ?? Default3,
				  service4 ?? Default4,
				  service5 ?? Default5,
				  service6 ?? Default6)
		{

		}
	}
	//</example>
}
