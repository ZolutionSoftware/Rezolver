// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    public class RequiresResolveContext
    {
        public IResolveContext Context { get; }
        public RequiresResolveContext(IResolveContext context)
        {
            Context = context;
        }
    }
}
