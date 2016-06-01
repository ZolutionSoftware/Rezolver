using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class GenericDecoratingHandler<T> : IHandler<T>
	{
		private readonly IHandler<T> _decorated;

		public GenericDecoratingHandler(IHandler<T> decorated)
		{
			_decorated = decorated;
		}

		public string Handle(T t)
		{
			return $"Decorated { _decorated.Handle(t) }";
		}
	}
}
