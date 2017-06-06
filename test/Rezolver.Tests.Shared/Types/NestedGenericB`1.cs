using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class NestedGenericB<T> : INestedGenericB<T>
	{
		public IEnumerable<T> Value
		{
			get; private set;
		}
	}
}
