// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ArrayExamples
    {
        [Fact]
        public void ShouldCreateEmptyArray()
        {
            // <example1>
            // this is fundamentally identical to the first example in the 
            // enumerables section
            var container = new Container();

            var result = container.Resolve<IMyService[]>();

            Assert.NotNull(result);
            Assert.Empty(result);

            // </example1>
        }

        [Fact]
        public void ShouldCreateAnArrayOfThreeItems()
        {
            // <example2>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            var result = container.Resolve<IMyService[]>();

            Assert.NotNull(result);
            Assert.Equal(3, result.Length);
            Assert.IsType<MyService1>(result[0]);
            Assert.IsType<MyService2>(result[1]);
            Assert.IsType<MyService3>(result[2]);
            // </example2>
        }

        [Fact]
        public void ShouldDisableArrayInjection()
        {
            // <example3>
            // create a clone of the default config
            // and then explicitly set the EnableArrayInjection option to false
            var config = TargetContainer.DefaultConfig
                .Clone() 
                .ConfigureOption<Options.EnableArrayInjection>(false);
            
            // now explicitly create a TargetContainer using this config
            var container = new Container(new TargetContainer(config));
            
            Assert.Throws<InvalidOperationException>(
                () => container.Resolve<IMyService[]>());
            // </example3>
        }
    }
}
