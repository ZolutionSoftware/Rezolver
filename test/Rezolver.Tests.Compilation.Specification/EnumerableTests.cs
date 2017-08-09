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
        // this specifically tests that the compiler is able to interpret the automatically created enumerables
        // which are exposed via the Targets.EnumerableTarget & EnumerableTargetContainer.
        
        // Both eager and lazy enumerables are tested
        [Fact]
        public void Enumerable_ShouldCreateEmpty()
        {
            var container = CreateContainer(CreateTargetContainer());

            var result = container.Resolve<IEnumerable<int>>();
            Assert.NotNull(result);
            Assert.False(result.Any());
        }

        [Fact]
        public void Enumerable_ShouldCreateLazyLoaded()
        {
            using (var session = InstanceCountingType.NewSession())
            {
                var targets = CreateTargetContainer();

                targets.RegisterType<InstanceCountingType>();
                targets.RegisterType<InstanceCountingType>();
                targets.RegisterType<InstanceCountingType>();

                var container = CreateContainer(targets);
                var result = container.Resolve<IEnumerable<InstanceCountingType>>();

                Assert.NotNull(result);
                Assert.Equal(0, session.InstanceCount);

                var array1 = result.ToArray();
                var array2 = result.ToArray();

                Assert.Equal(6, session.InstanceCount);
            }
        }

        [Fact]
        public void Enumerable_ShouldCreateEagerLoaded_GlobalOption()
        {
            // similar to above test, but this time, all instances should be created up front
            // and should not be recreated every time we enumerate.
            using (var session = InstanceCountingType.NewSession())
            {
                var targets = CreateTargetContainer();
                targets.SetOption<Options.LazyEnumerables>(false);

                targets.RegisterType<InstanceCountingType>();
                targets.RegisterType<InstanceCountingType>();
                targets.RegisterType<InstanceCountingType>();

                var container = CreateContainer(targets);
                var result = container.Resolve<IEnumerable<InstanceCountingType>>();

                Assert.NotNull(result);
                Assert.Equal(3, session.InstanceCount);

                var array1 = result.ToArray();
                var array2 = result.ToArray();

                Assert.Equal(3, session.InstanceCount);
            }
        }

        [Fact]
        public void Enumerable_ShouldCreateEagerLoaded_OneTypeOnly()
        {
            // Demonstrating that we can control lazy enumerables on a per-type basis
            using (var session1 = InstanceCountingType.NewSession())
            {
                using (var session2 = InstanceCountingType2.NewSession())
                {
                    var targets = CreateTargetContainer();
                    targets.SetOption<Options.LazyEnumerables, InstanceCountingType2>(false);

                    targets.RegisterType<InstanceCountingType>();
                    targets.RegisterType<InstanceCountingType>();
                    targets.RegisterType<InstanceCountingType>();

                    targets.RegisterType<InstanceCountingType2>();
                    targets.RegisterType<InstanceCountingType2>();
                    targets.RegisterType<InstanceCountingType2>();

                    var container = CreateContainer(targets);
                    var result1 = container.Resolve<IEnumerable<InstanceCountingType>>();
                    var result2 = container.Resolve<IEnumerable<InstanceCountingType2>>();

                    Assert.NotNull(result1);
                    Assert.NotNull(result2);
                    Assert.Equal(0, session1.InstanceCount);
                    Assert.Equal(3, session2.InstanceCount);

                    var array1a = result1.ToArray();
                    var array1b = result1.ToArray();
                    var array2a = result2.ToArray();
                    var array2b = result2.ToArray();

                    Assert.Equal(6, session1.InstanceCount);
                    Assert.Equal(3, session2.InstanceCount);
                }
            }
        }

        [Fact]
        public void Enumerable_Scoped_AllLazyInstancesShouldBeDisposed()
        {
            // if we enumerate a lazy enumerable multiple times (thus producing multiple items)
            // they should all be disposed when the enclosing scope is disposed
            var targets = CreateTargetContainer();
            targets.RegisterType<Disposable>();
            targets.RegisterType<Disposable2, Disposable>();
            targets.RegisterType<Disposable3, Disposable>();

            // try it with an explicitly constructed scope from a container.
            var container = CreateContainer(targets);
            List<Disposable> shouldBeDisposed = new List<Disposable>();
            using (var scope = container.CreateScope())
            {
                var result = scope.Resolve<IEnumerable<Disposable>>();
                // add the enumerable three times, should give 9 items
                shouldBeDisposed.AddRange(result);
                shouldBeDisposed.AddRange(result);
                shouldBeDisposed.AddRange(result);

                Assert.Equal(9, shouldBeDisposed.Count);
            }

            Assert.All(shouldBeDisposed, i =>
            {
                Assert.True(i.Disposed);
                Assert.Equal(1, i.DisposedCount);
            });
        }
    }
}
