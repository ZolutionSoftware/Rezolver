using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal interface ITwiceNestedGenericB<T> : IGeneric<IGeneric<IEnumerable<T>>>
	{

	}
}
