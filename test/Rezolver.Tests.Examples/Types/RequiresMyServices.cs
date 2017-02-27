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
		public static readonly MyService4 Default4 = new MyService4();
		public static readonly MyService5 Default5 = new MyService5();
		public static readonly MyService6 Default6 = new MyService6();
		#endregion

		#region Instance Properties
		public MyService1 Service1 { get; private set; }
		public MyService2 Service2 { get; private set; }
		public MyService3 Service3 { get; private set; }
		public MyService4 Service4 { get; private set; }
		public MyService5 Service5 { get; private set; }
		public MyService6 Service6 { get; private set; }
		#endregion

		public RequiresMyServices(MyService1 service1)
			: this(service1,
				  Default2,
				  Default3,
				  Default4,
				  Default5,
				  Default6)
		{

		}

		public RequiresMyServices(MyService1 service1,
			MyService2 service2,
			MyService3 service3,
			MyService4 service4,
			MyService5 service5,
			MyService6 service6)
		{
			Service1 = service1;
			Service2 = service2;
			Service3 = service3;
			Service4 = service4;
			Service5 = service5;
			Service6 = service6;
		}


	}
	//</example>
}
