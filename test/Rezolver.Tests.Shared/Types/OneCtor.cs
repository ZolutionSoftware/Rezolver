using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class OneCtor
	{
		public int Param1 { get; }

		public OneCtor(int param1)
		{
			Param1 = param1;
		}
	}
}
