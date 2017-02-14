using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class GenericOneCtor<T> : Generic<T>
    {
		public GenericOneCtor(T value)
		{
			Value = value;
		}
    }
}
