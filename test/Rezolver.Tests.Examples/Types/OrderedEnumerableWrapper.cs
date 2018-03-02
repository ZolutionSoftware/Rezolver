// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class OrderedEnumerableWrapper<T> : IOrderedEnumerable<T>
    {
        private readonly IOrderedEnumerable<T> _orderedEnumerable;

        // note - key selector and ascending/descending might also be desirable - but we'll
        // leave this out
        public OrderedEnumerableWrapper(IEnumerable<T> enumerable, IComparer<T> comparer)
        {
            _orderedEnumerable = enumerable.OrderBy(t => t, comparer);
        }

        IOrderedEnumerable<T> IOrderedEnumerable<T>.CreateOrderedEnumerable<TKey1>(
            Func<T, TKey1> keySelector, 
            IComparer<TKey1> comparer, 
            bool descending)
        {
            return _orderedEnumerable.CreateOrderedEnumerable(keySelector, comparer, descending);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _orderedEnumerable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _orderedEnumerable.GetEnumerator();
        }
    }
    // </example>
}
