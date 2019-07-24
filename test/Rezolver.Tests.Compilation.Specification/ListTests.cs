using Rezolver.Tests.Types;
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
        // similar to EnumerableTests

        // Tests that the compiler can successfully translate List<T> and IList<T> registrations which are automatically
        // added by the InjectLists configuration object
        [Fact]
        public void List_ShouldCreateEmpty_List()
        {
            // since the list injection is backed by the enumerable injection by default, it should now be possible
            // to inject an empty list without having to register any targets
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<List<int>>();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void List_ShouldCreateEmpty_IList()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<IList<int>>();
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void List_ShouldCreateEmpty_IReadOnlyList()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<IReadOnlyList<int>>();
            Assert.NotNull(result);
            Assert.Equal(0, result.Count);
        }

        private void RegisterBaseClassListAndCollTestTargets(ITargetContainer targets)
        {
            targets.RegisterType<BaseClass>();
            targets.RegisterType<BaseClassChild, BaseClass>();
            targets.RegisterType<BaseClassGrandchild, BaseClass>();
        }

        private Container Get3BaseClassItemsContainer()
        {
            var targets = CreateTargetContainer();

            RegisterBaseClassListAndCollTestTargets(targets);
            var container = CreateContainer(targets);
            return container;
        }

        [Fact]
        public void List_ShouldCreateWithItems_List()
        {
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<List<BaseClass>>();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result[1]);
            Assert.IsType<BaseClassGrandchild>(result[2]);
        }

        

        [Fact]
        public void List_ShouldCreateWithItems_IList()
        {
            // no choice but to repeat the above test, but for IList(!)
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<IList<BaseClass>>();

            // same base set of assertions as above.
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result[1]);
            Assert.IsType<BaseClassGrandchild>(result[2]);

            // BUT we also verify that we can modify the list (in case the compiler does something 'special'
            // for its list interface production, and since we only have a reference to an interface we can't
            // guarantee the implementation)
            result.Add(null);
            Assert.Equal(4, result.Count);
            result.Clear();
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void List_ShouldCreateWithItems_IReadOnlyList()
        {
            var container = Get3BaseClassItemsContainer();
            var result = container.Resolve<IReadOnlyList<BaseClass>>();

            // Again: same base set of assertions as above.
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.All(result, i => Assert.NotNull(i));
            Assert.IsType<BaseClassChild>(result[1]);
            Assert.IsType<BaseClassGrandchild>(result[2]);
        }
    }
}
