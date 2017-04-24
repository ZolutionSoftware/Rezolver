using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Rezolver
{
    internal class ReferenceComparer<T> : IEqualityComparer<T>
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
