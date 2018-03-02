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
    public class RequiresEnumerableOfServices
    {
        public IEnumerable<IMyService> Services { get; }
        public RequiresEnumerableOfServices(IEnumerable<IMyService> services)
        {
            Services = services;
        }
    }
    //</example>
}
