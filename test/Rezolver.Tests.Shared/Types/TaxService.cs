using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class TaxService
    {
		public decimal CalculateGross(decimal net, decimal taxRate)
		{
			return net + (net * taxRate);
		}
    }
}
