using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public interface IUsesAnyService<TAnyService>
    {
        void UseTheService(TAnyService service);
    }

    public class UsesAnyService<TAnyService> : IUsesAnyService<TAnyService>
    {
        public void UseTheService(TAnyService service)
        {
            throw new NotImplementedException();
        }
    }

    public class UsesAnyService2<TAnyService> : IUsesAnyService<TAnyService>
    {
        public void UseTheService(TAnyService service)
        {
            throw new NotImplementedException();
        }
    }
    //</example>
}
