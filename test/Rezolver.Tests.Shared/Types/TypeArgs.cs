using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	/// <summary>
	/// some types to use as generic arguments for our generic tests
	/// </summary>
	internal class TypeArgs
	{
		internal interface IT1 { }
		internal interface IT2 { }
		internal interface IT3 { }

		internal class T1 : IT1 { }
		internal class T2 : IT2 { }
		internal class T3 : IT3 { }

		internal struct VT1 : IT1 { }
		internal struct VT2 : IT2 { }
		internal struct VT3 : IT3 { }
	}
}
