using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class CollectionExamples
    {
        [Fact]
        public void ShouldCreateEmptyCollections()
        {
            // <example1>
            // this is fundamentally identical to the first example in the 
            // enumerables section
            var container = new Container();

            var result1 = container.Resolve<Collection<IMyService>>();
            var result2 = container.Resolve<ICollection<IMyService>>();
            var result3 = container.Resolve<ReadOnlyCollection<IMyService>>();
            var result4 = container.Resolve<IReadOnlyCollection<IMyService>>();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);
            Assert.Equal(0, result1.Count);
            Assert.Equal(0, result2.Count);
            Assert.Equal(0, result3.Count);
            Assert.Equal(0, result4.Count);
            // </example1>
        }

        [Fact]
        public void ShouldCreateCollectionsOfThreeItems()
        {
            // <example2>
            var container = new Container();
            container.RegisterType<MyService1, IMyService>();
            container.RegisterType<MyService2, IMyService>();
            container.RegisterType<MyService3, IMyService>();

            var result1 = container.Resolve<Collection<IMyService>>();
            var result2 = container.Resolve<ICollection<IMyService>>();
            var result3 = container.Resolve<ReadOnlyCollection<IMyService>>();
            var result4 = container.Resolve<IReadOnlyCollection<IMyService>>();

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
            Assert.NotNull(result4);

            Assert.Equal(3, result1.Count);
            Assert.Equal(3, result2.Count);
            Assert.Equal(3, result3.Count);
            Assert.Equal(3, result4.Count);

            Assert.IsType<MyService1>(result1[0]);
            Assert.IsType<MyService2>(result1[1]);
            Assert.IsType<MyService3>(result1[2]);

            Assert.IsType<MyService1>(result2.First());
            Assert.IsType<MyService2>(result2.Skip(1).First());
            Assert.IsType<MyService3>(result2.Skip(2).First());

            Assert.IsType<MyService1>(result3[0]);
            Assert.IsType<MyService2>(result3[1]);
            Assert.IsType<MyService3>(result3[2]);

            Assert.IsType<MyService1>(result4.First());
            Assert.IsType<MyService2>(result4.Skip(1).First());
            Assert.IsType<MyService3>(result4.Skip(2).First());
            // </example2>
        }

        [Fact]
        public void ShouldDisableAutoCollectionInjection()
        {
            // <example3>
            var targets = new TargetContainer(
                TargetContainer.DefaultConfig
                .Clone()
                .ConfigureOption<Options.EnableCollectionInjection>(false));

            var container = new Container(targets);

            Assert.Throws<InvalidOperationException>(
                () => container.Resolve<Collection<IMyService>>());
            // </example3>
        }
    }
}
