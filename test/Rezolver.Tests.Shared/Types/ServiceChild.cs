using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class ServiceChild
    {
		private int _input;

		public ServiceChild(int input)
		{
			_input = input;
		}

		public int Output {  get { return _input * 10; } }
    }
}
