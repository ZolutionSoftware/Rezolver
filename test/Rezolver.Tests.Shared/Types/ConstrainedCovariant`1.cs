using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Types
{
    public class ConstrainedCovariant<T> : ICovariant<T>
        where T : BaseClassChild
    {
        public T Out()
        {
            throw new NotImplementedException();
        }
    }
}
