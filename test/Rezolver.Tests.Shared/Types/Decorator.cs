using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Decorator : IDecorated
	{
		public IDecorated Decorated { get; }
		public Decorator(IDecorated decorated)
		{
			Decorated = decorated;
		}

		public string DoSomething()
		{
			return Decorated.DoSomething() + " World";
		}
	}
}
