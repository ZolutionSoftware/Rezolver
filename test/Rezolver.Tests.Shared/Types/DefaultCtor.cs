using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class DefaultCtor : NoCtor
    {
		public const int ExpectedValue = -1;
		public DefaultCtor()
		{
			Value = ExpectedValue;
		}
	}
}
