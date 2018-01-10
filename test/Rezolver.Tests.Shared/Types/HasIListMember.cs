using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class HasIListMember<T>
    {
        public IList<T> Children { get; }

        public HasIListMember()
        {
            Children = new List<T>();
        }
    }
}
