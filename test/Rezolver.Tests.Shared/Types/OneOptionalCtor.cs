using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class OneOptionalCtor : NoCtor
    {
		public const int ExpectedValue = 1;

		public OneOptionalCtor(int value = ExpectedValue)
		{
			Value = value;
		}
	}
}
