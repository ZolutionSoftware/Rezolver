using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class Contravariant<T> : IContravariant<T>
    {
        public void In(T t)
        {
            throw new NotImplementedException();
        }
    }
}
