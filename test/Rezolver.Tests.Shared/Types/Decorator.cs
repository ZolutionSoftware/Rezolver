using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Decorator : Decorated
	{
		public Decorated Decorated { get; }
		public Decorator(Decorated decorated)
		{
			Decorated = decorated;
		}
	}
}
