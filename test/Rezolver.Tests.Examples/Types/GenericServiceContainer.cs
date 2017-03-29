using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    public class GenericServiceContainer<TAnyService> : IServiceContainer<TAnyService>
    {
        public TAnyService Service { get; }

        public GenericServiceContainer(TAnyService service) => Service = service;
    }
}
