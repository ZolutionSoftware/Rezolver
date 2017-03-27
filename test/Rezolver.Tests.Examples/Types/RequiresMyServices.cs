using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class RequiresMyServices
	{
		#region Sentinel Instances
		public static readonly MyService2 Default2 = new MyService2();
		public static readonly MyService3 Default3 = new MyService3();
		#endregion

		#region Instance Properties
		public MyService1 Service1 { get; private set; }
		public MyService2 Service2 { get; private set; }
		public MyService3 Service3 { get; private set; }
		#endregion

		public RequiresMyServices(MyService1 service1)
			: this(service1,
				  Default2,
				  Default3)
		{

		}

		public RequiresMyServices(MyService1 service1,
			MyService2 service2,
			MyService3 service3)
		{
			Service1 = service1;
			Service2 = service2;
			Service3 = service3;
		}


	}
	//</example>
}
