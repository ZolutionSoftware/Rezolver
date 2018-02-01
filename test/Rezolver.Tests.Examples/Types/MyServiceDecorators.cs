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
    public class MyServiceDecorator1 : IMyService
    {
        public IMyService Inner { get; }
        public MyServiceDecorator1(IMyService inner)
        {
            Inner = inner;
        }
    }

    public class MyServiceDecorator2 : IMyService
    {
        public IMyService Inner { get; }
        public MyServiceDecorator2(IMyService inner)
        {
            Inner = inner;
        }
    }
    //</example>
}
