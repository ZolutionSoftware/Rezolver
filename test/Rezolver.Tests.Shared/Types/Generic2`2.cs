using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Generic2<Ta, Tb> : GenericBase<Ta>, IGeneric2<Ta, Tb>
	{
		public override void Foo()
		{

		}

		public void Foo2() { }
	}
}
