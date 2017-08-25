using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {

        [Fact]
        public void DecoratorDelegate_ShouldDecorateInt()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterDecoratorDelegate<int>(i => i * 2);
            var container = CreateContainer(targets);

            var result = container.Resolve<int>();
            Assert.Equal(2, result);
        }

        [Fact]
        public void DecoratorDelegate_ShouldDecorateEnumerableOfInt()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterDecoratorDelegate<IEnumerable<int>>(ii => ii.Concat(new[] { -1 }));
            var container = CreateContainer(targets);

            var result = container.Resolve<IEnumerable<int>>();
            Assert.Equal(new[] { 1, -1 }, result);
        }

        [Fact]
        public void DecoratorDelegate_ShouldStack()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(1);
            targets.RegisterDecoratorDelegate<int>(i => i * 2);
            targets.RegisterDecoratorDelegate<int>(i => i + 3);

            var container = CreateContainer(targets);

            var result = container.Resolve<int>();

            Assert.Equal(5, result);
        }

        [Fact]
        public void DecoratorDelegate_ShouldDecorateArrayAndIEnumerable()
        {
            var targets = CreateTargetContainer();
            targets.RegisterObject(2);
            targets.RegisterDecoratorDelegate<IEnumerable<int>>(ii => new[] { 1 }.Concat(ii));
            targets.RegisterDecoratorDelegate<int[]>(iii =>
            {
                var toReturn = new int[iii.Length + 1];
                Array.Copy(iii, toReturn, iii.Length);
                toReturn[2] = 2;
                return toReturn;
            });

            var container = CreateContainer(targets);

            var result = container.Resolve<int[]>();
        }
    }
}
