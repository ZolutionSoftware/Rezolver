using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	public class TwoCtorsOneNoOptional
	{
		//signatures here have to be slightly different obviously
		public string S { get; }
		public int I { get; }
		public object O { get; }
		public double D { get; }

		public TwoCtorsOneNoOptional(string s, int i, object o) { S = s; I = i; O = o; }
		public TwoCtorsOneNoOptional(string s, int i = 0, double d = 0) { S = s; I = i; D = d; }
	}
}
