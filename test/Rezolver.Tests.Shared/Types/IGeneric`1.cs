using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal interface IGeneric<T>
	{
		T Value { get; }
		void Foo();
	}
}
