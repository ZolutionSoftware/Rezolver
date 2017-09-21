using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class RectangleComparer : IComparer<Rectangle>
    {
        private readonly IComparer<I2DShape> _shapeComparer;
        private readonly IComparer<double> _dimensionComparer;

        public RectangleComparer(
            IComparer<I2DShape> shapeComparer,
            IComparer<double> dimensionComparer)
        {
            _shapeComparer = shapeComparer;
            _dimensionComparer = dimensionComparer;
        }

        public int Compare(Rectangle x, Rectangle y)
        {
            var result = _shapeComparer.Compare(x, y);
            if (result == 0)
            {
                result = _dimensionComparer.Compare(x.Length, y.Length);
                if(result == 0)
                {
                    result = _dimensionComparer.Compare(x.Height, y.Height);
                }
            }

            return result;
        }
    }
    // </example>

    public class RectangleComparerDecorator : IComparer<Rectangle>
    {
        private readonly IComparer<Rectangle> _shapeComparer;
        private readonly IComparer<double> _dimensionComparer;

        public RectangleComparerDecorator(
            IComparer<Rectangle> shapeComparer,
            IComparer<double> dimensionComparer)
        {
            _shapeComparer = shapeComparer;
            _dimensionComparer = dimensionComparer;
        }

        public int Compare(Rectangle x, Rectangle y)
        {
            var result = _shapeComparer.Compare(x, y);
            if (result == 0)
            {
                result = _dimensionComparer.Compare(x.Length, y.Length);
                if (result == 0)
                {
                    result = _dimensionComparer.Compare(x.Height, y.Height);
                }
            }

            return result;
        }
    }
}
