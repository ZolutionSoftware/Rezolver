// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class RequiresMyService : IRequiresIMyService
    {
        public MyService Service { get; }
        public RequiresMyService(MyService service)
        {
            Service = service;
        }

        public RequiresMyService(IMyService service)
        {
            if (service.GetType() != typeof(MyService))
            {
                throw new ArgumentException($"{ service.GetType() } not supported",
                    nameof(service));
            }
            Service = (MyService)service;
        }

        IMyService IRequiresIMyService.Service { get { return Service; } }
    }

    public class RequiresIMyService : IRequiresIMyService
    {
        public IMyService Service { get; }

        public RequiresIMyService(IMyService service)
        {
            Service = service;
        }
    }
    //</example>
}
