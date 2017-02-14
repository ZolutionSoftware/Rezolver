using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	//because of the type constraints on T, it's possible to map T in a generic ctor target
	//because we can simply replace it with the types in the constraints.

	public class ValidWideningGeneric<T, TElement> : IGeneric<TElement>
			where T : IEnumerable<TElement>
	{
		public TElement Value
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void Foo()
		{
			throw new NotImplementedException();
		}
	}
}
