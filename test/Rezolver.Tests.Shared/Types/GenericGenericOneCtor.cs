using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	//bit like a decorator, this.
    public class GenericGenericOneCtor<T> : GenericBase<IGeneric<T>>
    {
		public GenericGenericOneCtor(IGeneric<T> value)
		{
			Value = value;
		}
	}
}
