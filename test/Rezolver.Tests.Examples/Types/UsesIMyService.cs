using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class UsesIMyService : IUsesAnyService<IMyService>
    {
        public void UseTheService(IMyService service)
        {
            throw new NotImplementedException();
        }
    }
    public class UsesIMyService2 : IUsesAnyService<IMyService>
    {
        public void UseTheService(IMyService service)
        {
            throw new NotImplementedException();
        }
    }
    //</example>
}
