using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class UsesAnyServiceDecorator<TAnyService> : IUsesAnyService<TAnyService>
    {
        public IUsesAnyService<TAnyService> Inner { get; }

        public UsesAnyServiceDecorator(IUsesAnyService<TAnyService> inner)
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
