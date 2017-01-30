using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class GenericDecorator<TDecorated> : Decorated
		where TDecorated : Decorated
	{
		public TDecorated Decorated { get; }
		public GenericDecorator(TDecorated decorated)
		{
			Decorated = decorated;
		}
	}
}
