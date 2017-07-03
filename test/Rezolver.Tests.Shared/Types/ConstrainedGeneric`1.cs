using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class ConstrainedGeneric<T> : IGeneric<T>
        where T : BaseClass
    {
        public T Value { get; }

        public ConstrainedGeneric(T value)
        {
            Value = value;
        }
    }
}
