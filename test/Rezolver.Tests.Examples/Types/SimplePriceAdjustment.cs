using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    class SimplePriceAdjustmentConfig
    {
        public decimal? TriggerPrice { get; set; }
        public decimal Adjustment { get; set; }
        public bool IsPercentage { get; set; }
        public string DisplayName { get; set; }
    }

    interface IPriceAdjustment
    {
        decimal Apply(decimal inputPrice, decimal originalPrice);
    }

    class SimplePriceAdjustment : IPriceAdjustment
    {
        readonly SimplePriceAdjustmentConfig _config;
        public SimplePriceAdjustment(SimplePriceAdjustmentConfig config)
        {
            _config = config;
        }

        public virtual decimal Apply(decimal inputPrice, decimal originalPrice)
        {
            if (originalPrice >= (_config.TriggerPrice ?? originalPrice))
            {
                if (_config.IsPercentage)
                    return inputPrice * _config.Adjustment;
                else
                    return inputPrice + _config.Adjustment;
            }

            return inputPrice;
        }
    }
    // </example>

    // <example2>
    class NeverLessThanHalfPrice : IPriceAdjustment
    {
        readonly IPriceAdjustment _inner;
        public NeverLessThanHalfPrice(IPriceAdjustment inner)
        {
            _inner = inner;
        }

        public decimal Apply(decimal inputPrice, decimal originalPrice)
        {
            var innerResult = _inner.Apply(inputPrice, originalPrice);
            if (innerResult < (originalPrice / 2))
                return inputPrice;
            return innerResult;
        }
    }
    // </example2>
}
