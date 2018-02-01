// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public class RequiresIMyServiceAndDateTime
    {
        public DateTime StartDate { get; }
        public IMyService Service { get; }

        public RequiresIMyServiceAndDateTime(IMyService service)
            : this(service, DateTime.UtcNow)
        {

        }

        public RequiresIMyServiceAndDateTime(IMyService service, DateTime startDate)
        {
            Service = service;
            StartDate = startDate;
        }
    }
    //</example>
}
