using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class TwiceNestedGenericB<T> : ITwiceNestedGenericB<T>
	{
		public IGeneric<IEnumerable<T>> Value
		{
			get; private set;
		}

		public void Foo()
		{
			throw new NotImplementedException();
		}
	}
}
