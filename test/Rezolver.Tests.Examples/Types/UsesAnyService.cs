// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

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
