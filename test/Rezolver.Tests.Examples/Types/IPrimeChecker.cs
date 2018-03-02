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
    public interface IPrimeChecker
    {
        bool IsPrime(int i);
    }

    public class PrimesUnder20Checker : IPrimeChecker
    {
        static readonly HashSet<int> PrimesUnderTwenty =
            new HashSet<int>(new[]
            {
                2, 3, 5, 7, 11, 13, 17, 19
            });

        public bool IsPrime(int i)
        {
            if (i == 0 || i == 1)
                return false;
            else if (i > 1 && i < 20)
                return PrimesUnderTwenty.Contains(i);
            else
                throw new ArgumentOutOfRangeException(nameof(i));
        }
    }
    // </example>
}
