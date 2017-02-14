using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class OMGDecorator : IDecorated
    {
		IDecorated _decorated;
		public OMGDecorator(IDecorated decorated)
		{
			_decorated = decorated;
		}

		public string DoSomething()
		{
			return "OMG: " + _decorated.DoSomething();
		}
	}
}
