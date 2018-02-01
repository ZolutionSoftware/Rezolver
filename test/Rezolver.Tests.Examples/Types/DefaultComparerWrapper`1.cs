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
    public class DefaultComparerWrapper<T> : IComparer<T>
    {
        int IComparer<T>.Compare(T x, T y)
        {
            return Comparer<T>.Default.Compare(x, y);
        }
    }
    // </example>
}
