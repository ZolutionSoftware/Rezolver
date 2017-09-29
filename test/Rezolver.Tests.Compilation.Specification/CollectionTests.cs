using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        // very similar to the tests in ListTests

        [Fact]
        public void Collection_ShouldCreateEmpty_Collection()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<Collection<int>>();
            
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Collection_ShouldCreateEmpty_ICollection()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<ICollection<int>>();

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Collection_ShouldCreateEmpty_ROCollection()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<ReadOnlyCollection<int>>();

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Collection_ShouldCreateEmpty_IROCollection()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<IReadOnlyCollection<int>>();

            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Collection_ShouldCreateWithItems_Collection()
        {
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<Collection<BaseClass>>();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result[1]);
            Assert.IsType<BaseClassGrandchild>(result[2]);
        }

        [Fact]
        public void Collection_ShouldCreateWithItems_ICollection()
        {
            // no choice but to repeat the above test, but for IList(!)
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<ICollection<BaseClass>>();

            // same base set of assertions as above.
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result.ElementAt(1));
            Assert.IsType<BaseClassGrandchild>(result.ElementAt(2));

            // BUT we also verify that we can modify the list (in case the compiler does something 'special'
            // for its list interface production, and since we only have a reference to an interface we can't
            // guarantee the implementation)
            result.Add(null);
            Assert.Equal(4, result.Count);
            result.Clear();
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void Collection_ShouldCreateWithItems_ReadOnlyCollection()
        {
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<ReadOnlyCollection<BaseClass>>();

            // Again: same base set of assertions as above.
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result[1]);
            Assert.IsType<BaseClassGrandchild>(result[2]);
        }

        [Fact]
        public void Collection_ShouldCreateWithItems_IReadOnlyCollection()
        {
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<IReadOnlyCollection<BaseClass>>();

            // Again: same base set of assertions as above.
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result.ElementAt(1));
            Assert.IsType<BaseClassGrandchild>(result.ElementAt(2));
        }
    }
}
