using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Types
{
    public class Decorator2 : IDecorated
    {
		public IDecorated Decorated { get; }
		public Decorator2(IDecorated decorated)
		{
            Assert.NotNull(decorated);
			Decorated = decorated;
		}
	}
}
