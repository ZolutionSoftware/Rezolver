using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresTaxService
    {
		private readonly TaxService _taxService;
		private readonly decimal _taxRate;
		public RequiresTaxService(TaxService taxService, decimal taxRate)
		{
			_taxService = taxService;
			_taxRate = taxRate;
		}

		public decimal CalculatePrice(decimal itemPrice)
		{
			return _taxService.CalculateGross(itemPrice, _taxRate);
		}
    }
}
