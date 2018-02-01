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
    public class CovarianceExamples
    {
        [Fact]
        public void ShouldFetchCovariantMatch()
        {
            // <example1>
            var container = new Container();
            container.RegisterObject<Func<MyService2>>(() => new MyService2());

            var result = container.Resolve<Func<IMyService>>();

            Assert.IsType<Func<MyService2>>(result);
            // </example1>
        }

        [Fact]
        public void ShouldFetchIEnumerableOfCovariantMatches()
        {
            // <example2>
            var container = new Container();
            container.RegisterType<MyService1>();
            container.RegisterType<MyService2>();
            container.RegisterType<MyService3>();

            var result = container.ResolveMany<IMyService>();

            Assert.Collection(result,
                new Action<IMyService>[]
                {
                    s => Assert.IsType<MyService1>(s),
                    s => Assert.IsType<MyService2>(s),
                    s => Assert.IsType<MyService3>(s)
                });
            // </example2>
        }

        [Fact]
        public void ShouldFetchIEnumerableOfNestedCovariantMatches()
        {
            // <example3>
            var container = new Container();
            container.RegisterObject<Func<MyService1>>(() => new MyService1());
            container.RegisterObject<Func<MyService2>>(() => new MyService2());
            container.RegisterObject<Func<MyService3>>(() => new MyService3());

            var result = container.ResolveMany<Func<IMyService>>();

            Assert.Collection(result,
                new Action<Func<IMyService>>[]
                {
                    f => Assert.IsType<Func<MyService1>>(f),
                    f => Assert.IsType<Func<MyService2>>(f),
                    f => Assert.IsType<Func<MyService3>>(f)
                });
            // </example3>
        }

        [Fact]
        public void ShouldFetchIEnumerableOfNestedContravariantMatches()
        {
            // <example4>
            var container = new Container();
            container.RegisterObject<Action<IMyService>>(s => Console.WriteLine("interface"));
            container.RegisterObject<Action<object>>(s => Console.WriteLine("object"));

            var result = container.ResolveMany<Action<MyService>>();

            Assert.Collection(result,
                new Action<Action<MyService>>[]{
                    a => Assert.IsType<Action<IMyService>>(a),
                    a => Assert.IsType<Action<object>>(a)
                });
            // </example4>
        }
    }
}
