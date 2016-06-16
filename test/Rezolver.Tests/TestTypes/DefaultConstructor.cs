using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.TestTypes
{
	public class DefaultConstructor : NoExplicitConstructor
	{
		public const int ExpectedValue = -1;
		public DefaultConstructor()
		{
			Value = ExpectedValue;
		}
	}
}
