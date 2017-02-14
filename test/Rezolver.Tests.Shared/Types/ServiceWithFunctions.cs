using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class ServiceWithFunctions
    {
		public ServiceChild GetChild(int input)
		{
			return new ServiceChild(input);
		}
    }
}
