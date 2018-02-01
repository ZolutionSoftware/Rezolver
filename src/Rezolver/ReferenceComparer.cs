// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rezolver
{
    internal class ReferenceComparer<T> : IEqualityComparer<T>
        where T:class
    {
        public static IEqualityComparer<T> Instance { get; } = new ReferenceComparer<T>();

        private ReferenceComparer() { }

        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj != null ? RuntimeHelpers.GetHashCode(obj) : 0;
        }
    }
}
