using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class GenericDecoratingHandler2<T> : IHandler<T>
	{
		private readonly IHandler<T> _decorated;

		public GenericDecoratingHandler2(IHandler<T> decorated)
		{
			_decorated = decorated;
		}
		public string Handle(T t)
		{
			return $"({ _decorated.Handle(t) }) Decorated again :)";
		}
	}
}
