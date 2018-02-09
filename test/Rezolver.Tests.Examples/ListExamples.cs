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
    public class ListExamples
    {
        [Fact]
        public void ShouldCreateEmptyLists()
        {
            // <example1>
            var container = new Container();

            var result1 = container.Resolve<List<IMyService>>();
            var result2 = container.Resolve<IList<IMyService>>();
            var result3 = container.Resolve<IReadOnlyList<IMyService>>();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            Assert.Empty(result1);
            Assert.Equal(0, result2.Count);
            Assert.Equal(0, result3.Count);
            // </example1>
        }

        [Fact]
        public void ShouldCreateListsOfThreeItems()
        {
            // <example2>
            var container = new Container();

            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            var result1 = container.Resolve<List<IMyService>>();
            var result2 = container.Resolve<IList<IMyService>>();
            var result3 = container.Resolve<IReadOnlyList<IMyService>>();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);

            Assert.Equal(3, result1.Count);
            Assert.Equal(3, result2.Count);
            Assert.Equal(3, result3.Count);

            Assert.IsType<MyService1>(result1[0]);
            Assert.IsType<MyService2>(result1[1]);
            Assert.IsType<MyService3>(result1[2]);

            Assert.IsType<MyService1>(result2[0]);
            Assert.IsType<MyService2>(result2[1]);
            Assert.IsType<MyService3>(result2[2]);

            Assert.IsType<MyService1>(result3[0]);
            Assert.IsType<MyService2>(result3[1]);
            Assert.IsType<MyService3>(result3[2]);
            // </example2>
        }

        [Fact]
        public void ShouldDisableAutoListInjection()
        {
            // <example3>
            var container = new Container(
                new TargetContainer(
                    TargetContainer.DefaultConfig
                    .Clone()
                    .ConfigureOption<Options.EnableListInjection>(false)));

            Assert.Throws<InvalidOperationException>(
                () => container.Resolve<List<IMyService>>());
            // </example3>
        }

        [Fact]
        public void ShouldDecorateListWithDisposableListDecorator()
        {
            // <example10>
            var container = new Container();
            container.RegisterDecorator(typeof(DisposableListDecorator<>), typeof(IList<>));

            // Note above - we only register the decorator, the underlying List<>
            // is always created by Rezolver from its own default behaviour

            var list = container.Resolve<IList<object>>();

            var toDispose = new DisposableType();
            list.Add(toDispose);
            list.Clear(); //disposes here

            Assert.True(toDispose.Disposed);
            // </example10>
        }
    }
}
