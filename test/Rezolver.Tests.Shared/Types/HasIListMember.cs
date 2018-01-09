using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class HasIListMember
    {
        public IList<int> ListOfInts { get; }

        public HasIListMember()
        {
            ListOfInts = new List<int>();
        }
    }
}
