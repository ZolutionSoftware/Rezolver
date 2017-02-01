using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    internal class AltGeneric<T> : GenericBase<T>
    {
		public override void Foo()
		{
			throw new NotImplementedException();
		}
	}
}
