// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class AcceptsOptionalIMyService
    {
        public IMyService Service { get; }
        public AcceptsOptionalIMyService()
        {

        }

        public AcceptsOptionalIMyService(IMyService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            Service = service;
        }
    }
    //</example>
}
