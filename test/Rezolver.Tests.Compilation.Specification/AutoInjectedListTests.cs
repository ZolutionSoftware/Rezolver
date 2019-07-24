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
        private Container PrepareAutoListContainer()
        {
            //using the default configuration
            var targets = new TargetContainer();
            //should yield a list with three items.
            targets.RegisterType<Types.DefaultCtor, Types.NoCtor>();
            targets.RegisterType<Types.DefaultCtor, Types.NoCtor>();
            targets.RegisterType<Types.DefaultCtor, Types.NoCtor>();

            var container = CreateContainer(targets);
            return container;
        }

        [Fact]
        public void AutoLists_ShouldResolveListByDefault()
        {
            Container container = PrepareAutoListContainer();

            var result = container.Resolve<List<Types.NoCtor>>();

            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.Equal(result, result.Distinct());
        }


        [Fact]
        public void AutoLists_ShouldResolveIListByDefault()
        {
            //same test really, except this time resolving by IList<T>
            Container container = PrepareAutoListContainer();

            var result = container.Resolve<IList<Types.NoCtor>>();

            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.Equal(result, result.Distinct());
        }

        [Fact]
        public void AutoLists_ShouldResolveIReadOnlyListByDefault()
        {
            //same test really, except this time resolving by IList<T>
            Container container = PrepareAutoListContainer();

            var result = container.Resolve<IReadOnlyList<Types.NoCtor>>();

            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.Equal(result, result.Distinct());
        }
    }
}
