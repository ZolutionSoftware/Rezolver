﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System.Collections.Generic;

namespace System.Linq
{
    internal static class NestedEnumerableExtensions
    {
        internal static IEnumerable<IEnumerable<T>> Permutate<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            // thank you Eric Lippert...
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
        }
    }
}
