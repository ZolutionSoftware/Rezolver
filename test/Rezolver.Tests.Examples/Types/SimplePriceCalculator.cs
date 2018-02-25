using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    class SimplePriceCalculator
    {
        readonly IEnumerable<SimplePriceAdjustment> _adjustments;

        public SimplePriceCalculator(IEnumerable<SimplePriceAdjustment> adjustments)
        {
            _adjustments = adjustments;
        }

        public decimal Calculate(decimal itemPrice)
        {
            decimal original = itemPrice;
            foreach (var adjustment in _adjustments)
            {
                itemPrice = adjustment.Apply(itemPrice, original);
            }
            return itemPrice;
        }
    }
    // </example>

    // <example2>
    class PriceCalculator
    {
        readonly IEnumerable<IPriceAdjustment> _adjustments;

        public PriceCalculator(IEnumerable<IPriceAdjustment> adjustments)
        {
            _adjustments = adjustments;
        }

        public decimal Calculate(decimal itemPrice)
        {
            decimal original = itemPrice;
            foreach (var adjustment in _adjustments)
            {
                itemPrice = adjustment.Apply(itemPrice, original);
            }
            return itemPrice;
        }
    }
    // </example2>
}
