using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Runtime
{
    internal class TargetIdentityComparer : IEqualityComparer<ITarget>
    {
        internal static TargetIdentityComparer Instance { get; } = new TargetIdentityComparer();
        private TargetIdentityComparer() { }
        public bool Equals(ITarget x, ITarget y)
        {
            return x?.Id == y?.Id;
        }

        public int GetHashCode(ITarget obj)
        {
            return ((obj?.Id) ?? Guid.Empty).GetHashCode();
        }
    }
}
