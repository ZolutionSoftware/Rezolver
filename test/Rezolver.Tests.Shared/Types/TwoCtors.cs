using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class TwoCtors
	{
		public string S { get; }
		public int I { get; }

		public TwoCtors(string s) { S = s; }
		public TwoCtors(string s, int i) { S = s; I = i; }
	}
}
