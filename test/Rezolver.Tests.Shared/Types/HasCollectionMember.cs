using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Rezolver.Tests.Types
{
    public class HasCollectionMember
    {
        public ICollection<int> Numbers { get; }

        public HasCollectionMember()
        {
            Numbers = new Collection<int>();
        }
    }
}
