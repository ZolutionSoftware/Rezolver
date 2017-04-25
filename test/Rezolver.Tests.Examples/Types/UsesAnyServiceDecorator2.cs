using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class UsesAnyServiceDecorator2<TAnyService> : IUsesAnyService<TAnyService>
    {
        public IUsesAnyService<TAnyService> Inner { get; }

        public UsesAnyServiceDecorator2(IUsesAnyService<TAnyService> inner)
        {
            Inner = inner;
        }

        public void UseTheService(TAnyService service)
        {
            throw new NotImplementedException();
        }
    }
    // </example>
}
