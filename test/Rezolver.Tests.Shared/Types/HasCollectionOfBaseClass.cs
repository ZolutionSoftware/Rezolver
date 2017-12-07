using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class HasCollectionOfBaseClass
    {
        public Collection<BaseClass> Collection { get; } = new Collection<BaseClass>();
    }
}
