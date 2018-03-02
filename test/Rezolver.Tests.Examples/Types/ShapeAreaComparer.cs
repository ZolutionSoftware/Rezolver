// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class ShapeAreaComparer : IComparer<I2DShape>
    {
        private readonly IComparer<double> _doubleComparer;

        public ShapeAreaComparer(IComparer<double> doubleComparer)
        {
            _doubleComparer = doubleComparer;
        }
        public int Compare(I2DShape x, I2DShape y)
        {
            return _doubleComparer.Compare(x.CalcArea(), y.CalcArea());
        }
    }
    // </example>
}
