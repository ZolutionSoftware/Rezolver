using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class AnotherDecoratorType : IDecorated
	{
		IDecorated _decorated;
		public AnotherDecoratorType(IDecorated decorated)
		{
			_decorated = decorated;
		}

		public string DoSomething()
		{
			return "OMG: " + _decorated.DoSomething();
		}
	}
}
