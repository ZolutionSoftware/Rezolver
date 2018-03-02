// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    // Slightly different to before properties are IMyService, but
    // constructors use concrete types.
    public class Requires2MyServices
    {
        public IMyService First { get; }
        public IMyService Second { get; }

        public Requires2MyServices(MyService1 first)
        {
            First = first;
            Second = null;
        }

        public Requires2MyServices(MyService2 first,
            MyService3 second)
        {
            First = first;
            Second = second;
        }
    }
    //</example>
}
