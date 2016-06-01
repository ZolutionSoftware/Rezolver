using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public interface IHandler<T>
	{
		string Handle(T t);
	}
}
