// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ObjectLifetimeExamples
    {
        [Fact]
        public void ShouldCreateInstanceOfMyService()
        {
            // <MyServiceDirect>
            Container container = new Container();
            container.RegisterType<MyService>();
            var result = container.Resolve<MyService>();
            Assert.NotNull(result);
            // </MyServiceDirect>
        }
    }
}
