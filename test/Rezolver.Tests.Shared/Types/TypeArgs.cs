using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	/// <summary>
	/// some types to use as generic arguments for our generic tests
	/// </summary>
	public class TypeArgs
	{
		public interface IT1 { }
		public interface IT2 { }
		public interface IT3 { }

		public class T1 : IT1 { }
		public class T2 : IT2 { }
		public class T3 : IT3 { }

		public struct VT1 : IT1 { }
		public struct VT2 : IT2 { }
		public struct VT3 : IT3 { }
	}
}
