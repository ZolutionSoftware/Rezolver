using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	//two param generic - note that you can't get an instance
	//by requesting IGeneric<[sometype]> because there's no way to know
	//the type argument to use for the other parameters.
	public interface IGeneric2<Ta, Tb> : IGeneric<Ta>
	{
		Tb ValueB { get; }
	}
}
