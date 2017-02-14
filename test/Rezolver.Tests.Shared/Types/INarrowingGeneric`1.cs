using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	//the generic type you request from the generic constructor target must have at
	//least as many generic arguments as the type to which its bound. 
	//I.E you can't register Foo<T,U> : IBar<T> to IBar<> because when you request 
	//IBar<Baz>, the system cannot bind the 'U' type parameter for Foo<,>
	//However, if you register Bar<T> : IFoo<T, int> for IFoo<,> then you can request 
	//IFoo<double, int>, because the second type parameter is statically mapped.

	public interface INarrowingGeneric<T> : IGeneric2<T, TypeArgs.T2>
	{

	}
}
