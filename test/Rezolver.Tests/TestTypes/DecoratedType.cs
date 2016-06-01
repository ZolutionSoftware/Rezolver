using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class DecoratedType : IDecorated
	{
		public string DoSomething()
		{
			return "Hello";
		}
	}
}
