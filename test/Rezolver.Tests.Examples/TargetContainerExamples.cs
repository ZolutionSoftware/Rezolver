// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class TargetContainerExamples
    {
        public void SimpleLookup()
        {
            // <example1>
            var targets = new TargetContainer();
            targets.RegisterObject("hello world");

            var target = targets.Fetch(typeof(string));
            Assert.IsType<ObjectTarget>(target);
            // </example1>
        }

        public void LookupByBase()
        {
            // <example2>
            var targets = new TargetContainer();
            targets.Register(Target.ForType<MyService>(), typeof(IMyService));

            var target = targets.Fetch(typeof(IMyService));
            Assert.IsType<ConstructorTarget>(target);
            // </example2>
        }

        public void WillRejectBecauseIncompatibleType()
        {
            // <example3>
            var targets = new TargetContainer();
            // int is obviously not compatible with IMyService.
            Assert.Throws<ArgumentException>(
                () => targets.Register(Target.ForObject(50), typeof(IMyService)));
            // </example3>
        }
    }
}
