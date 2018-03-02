// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public class UsesMyService2Decorator : IUsesAnyService<MyService2>
    {
        public IUsesAnyService<MyService2> Inner { get; }

        public UsesMyService2Decorator(IUsesAnyService<MyService2> inner)
        {
            Inner = inner;
        }

        public void UseTheService(MyService2 service)
        {
            throw new NotImplementedException();
        }
    }
    // </example>
}
