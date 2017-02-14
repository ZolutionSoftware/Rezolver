using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresIGenericT<T> : GenericOneCtor<IGeneric<T>>
    {
		public RequiresIGenericT(IGeneric<T> value)
			: base(value)
		{
			
		}
    }
}
