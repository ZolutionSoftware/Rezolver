using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class NestedGenericA<T> : Generic<IEnumerable<T>>
	{
		//finding this type's mapping to any of the bases or interfaces involves 
		//translating through the layers of inheritance/implementation and 
		//getting to the IEnumerable type parameter, then lifting the T out
	}
}
