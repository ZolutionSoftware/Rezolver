using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Targets
{
	public class NoCtor { }

	public class OneCtor
	{
		public OneCtor(int param1)
		{

		}
	}

	public class TwoCtors
	{
		public TwoCtors(string s) { }
		public TwoCtors(string s, int i) { }
	}

	public class TwoCtorsOneNoOptional
	{
		//signatures here have to be slightly different obviously

		public TwoCtorsOneNoOptional(string s, int i, object o) { }
		public TwoCtorsOneNoOptional(string s, int i = 0, double d = 0) { }
	}
}
