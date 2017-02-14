using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class RequiresInt : IRequiresInt
	{
		public int IntValue { get; private set; }
		public RequiresInt(int intValue)
		{
			IntValue = intValue;
		}
	}
}
