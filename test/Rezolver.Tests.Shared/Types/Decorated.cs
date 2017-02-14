using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class Decorated : IDecorated
	{
		public string DoSomething()
		{
			return "Hello";
		}
	}
}
