using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class NarrowingGeneric<T> : Generic2<T, TypeArgs.T2>, INarrowingGeneric<T>
	{
		//if you register this type and request an IGeneric<*, T2> it should work.
		//equally, if you request an IGeneric<*> it should also work.
	}
}
