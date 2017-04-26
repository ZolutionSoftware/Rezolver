using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests
{

    public class ContainerBehaviourCollectionTests
    {
        ITestOutputHelper Output { get; }
        public ContainerBehaviourCollectionTests(ITestOutputHelper output)
        {
            Output = output;
        }
        // Note - this test also tests the lower-level DependantCollection as everything we're
        // doing here relies on that.  The ContainerBehaviourCollection class itself is just a couple of
        // connstructors and a stub implementation of another which always returns an empty enumerable.

        [Fact]
        public void ShouldBeEmptyOnConstruction()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            Assert.Empty(behaviours);
        }

        [Fact]
        public void ShouldAddBehaviour()
        {
            // behaviours can be added and then should appear in the IEnumerable
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var behaviour = new TestBehaviour();
            behaviours.Add(behaviour);
            Assert.Contains(behaviour, behaviours);
        }

        [Fact]
        public void ShouldAddAllBehaviours()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var expected = new IContainerBehaviour[]
            {
                new TestBehaviour(), new TestBehaviour2()
            };

            behaviours.AddAll(expected);

            Assert.Equal(expected, behaviours);
        }

        [Fact]
        public void ShouldAddAllBehavioursOnConstruction()
        {
            var expected = new IContainerBehaviour[]
            {
                new TestBehaviour(), new TestBehaviour2()
            };
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection(expected);

            Assert.Equal(expected, behaviours);
        }

        [Fact]
        public void ShouldRemoveBehaviour()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var behaviour = new TestBehaviour();
            behaviours.Add(behaviour);
            behaviours.Remove(behaviour);
            Assert.Empty(behaviours);
        }

        [Fact]
        public void ShouldRemoveBehaviours()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var toRemove = new IContainerBehaviour[]
            {
                new TestBehaviour(),
                new TestBehaviour(),
                new TestBehaviour()
            };
            var remaining = new IContainerBehaviour[]
            {
                new TestBehaviour2()
            };

            behaviours.AddAll(toRemove);
            behaviours.AddAll(remaining);
            behaviours.RemoveAll(toRemove);

            Assert.Equal(remaining, behaviours);
        }

        [Fact]
        public void ShouldReplaceBehaviour()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var original = new TestBehaviour();
            var replacement = new TestBehaviour2();

            behaviours.Add(original);
            behaviours.Replace(original, replacement);

            Assert.Equal(new[] { replacement }, behaviours);
        }

        [Fact]
        public void ShouldConfigureAddedBehaviour()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            bool configured = false;

            behaviours.Add(new TestBehaviour((c, t) => configured = true));

            behaviours.Configure(null, null);
            Assert.True(configured);
        }

        [Fact]
        public void ShouldConfigureAllAddedBehaviours()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            int configured = 0;

            behaviours.Add(new TestBehaviour((c, t) => configured++));
            behaviours.Add(new TestBehaviour((c, t) => configured++));
            behaviours.Add(new TestBehaviour((c, t) => configured++));

            behaviours.Configure(null, null);

            Assert.Equal(3, configured);
        }

        [Fact]
        public void ShouldConfigureBehavioursInRegistrationOrder()
        {
            // behaviours must be applied in the order they're registered,
            // assuming no dependencies etc.
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            bool firstCalled = true;
            bool secondCalled = true;

            behaviours.Add(new TestBehaviour((c, t) => firstCalled = true));
            behaviours.Add(new TestBehaviour((c, t) =>
            {
                Assert.True(firstCalled);
                secondCalled = true;
            }));

            behaviours.Configure(null, null);
            Assert.True(secondCalled);
        }

        // Dependency ordering tests.  Candidates for theories, clearly - but these will do for now.

        static IEnumerable<IEnumerable<T>> UniqueCartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            //thank you Eric Lippert...
            IEnumerable<IEnumerable<T>> emptyProduct =
              new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                where !accseq.Contains(item)
                select accseq.Concat(new[] { item }));
        }

        IEnumerable<IEnumerable<T>> UniquePermutations<T>(T[] baseArray)
        {
            var indices = Enumerable.Range(0, baseArray.Length).ToArray();
            var ranges = indices.Select(i => indices);
            //not the most efficient way of producing unique permutations - but it's fine for testing
            return UniqueCartesianProduct(ranges)
                .Select(r => {
                    return r.Select(i => baseArray[i]).ToArray();
                });
        }

        private void RunBehaviourTest(IContainerBehaviour[] behaviours, Action<ContainerBehaviourCollection> test = null, Action reset = null, bool dontConfigure=false)
        {
            // runs the test for each unique permutation of the behaviours.
            // note that the test fails as soon as one order fails - and, because
            // I'm not proficient enouigh with Xunit yet - I'm just outputting the 
            // indices of the items, in order, which led to the failure.
            // Remember - keep the number of input items to a reasonable amount; with a
            // suggested maximum of 9-10 as the number of permutations is (n!) where n is the 
            // length of the behaviours array.
            int count = 0;
            foreach (var uniqueOrder in UniquePermutations(behaviours))
            {
                count++;
                var collection = new ContainerBehaviourCollection(uniqueOrder);
                try
                {
                    if(!dontConfigure) collection.Configure(null, null);
                    test?.Invoke(collection);
                    reset?.Invoke();
                }
                catch (Exception)
                {
                    Output.WriteLine($"Ordering of failed items: { string.Join(", ", uniqueOrder.Select(i => Array.IndexOf(behaviours, i))) } ");
                    throw;
                }
            }

            Output.WriteLine($"Total number of permutations run: { count }");
        }

        [Fact]
        public void ShouldConfigureDependencyBeforeDependantBehaviour()
        {
            // The IDependantBehaviour interface provides the ability for behaviours to declare 
            // a dependency on other behaviours.  The behaviours collection automatically processes
            // those dependencies when Configure() is called 
            bool rootCalled = false, dependantCalled = false;
            var root = new TestBehaviour((c, t) => rootCalled = true);
            var dependant = new TestBehaviour((c, t) =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            });
            dependant.Requires(root);

            RunBehaviourTest(new IContainerBehaviour[] { root, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldConfigureTransitiveDependencyBeforeDependants()
        {
            bool rootCalled = false, midCalled = false, dependantCalled = false;

            var root = new TestBehaviour((c, t) => rootCalled = true);
            var mid = new TestBehaviour((c, t) =>
            {
                Assert.True(rootCalled);
                midCalled = true;
            }).Requires(root);
            var dependant = new TestBehaviour((c, t) =>
            {
                Assert.True(midCalled);
                dependantCalled = true;
            }).Requires(mid);

            RunBehaviourTest(new[] { root, mid, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = midCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldConfigureDependencyBeforeAllDependants()
        {
            bool rootCalled = false;
            int dependantCalled = 0;
            var root = new TestBehaviour((c, t) => rootCalled = true);
            var dependantCallBack = new Action<IContainer, ITargetContainer>((c, t) =>
            {
                Assert.True(rootCalled);
                dependantCalled++;
            });

            RunBehaviourTest(new[] {
                    root,
                    new TestBehaviour(dependantCallBack).Requires(root),
                    new TestBehaviour(dependantCallBack).Requires(root),
                    new TestBehaviour(dependantCallBack).Requires(root)
                },
                c => Assert.Equal(3, dependantCalled),
                () =>
                {
                    rootCalled = false;
                    dependantCalled = 0;
                });
        }

        [Fact]
        public void ShouldConfigureDependantsWithIndependents()
        {
            bool rootCalled = false;
            bool dependantCalled = false;
            bool independentCalled = false;
            var root = new TestBehaviour((c, t) => rootCalled = true);
            var dependant = new TestBehaviour((c, t) =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            }).Requires(root);
            var independent = new TestBehaviour((c, t) => independentCalled = true);

            RunBehaviourTest(new[] { dependant, independent, root },
                c =>
                {
                    Assert.True(dependantCalled);
                    Assert.True(independentCalled);
                },
                () => rootCalled = dependantCalled = independentCalled = false);           
        }

        [Fact]
        public void ShouldConfigureDependantsOfTwoGroupsOfRelatedBehaviours()
        {
            bool[] rootCalled = { false, false };
            bool[] midCalled = { false, false };
            bool[] dependantCalled = { false, false };
            bool[] independentCalled = { false, false };

            //group1 of dependency chain
            var root1 = new TestBehaviour((c, t) => rootCalled[0] = true);
            var mid1 = new TestBehaviour((c, t) =>
            {
                Assert.True(rootCalled[0]);
                midCalled[0] = true;
            }).Requires(root1);
            var dependant1 = new TestBehaviour((c, t) =>
            {
                Assert.True(midCalled[0]);
                dependantCalled[0] = true;
            }).Requires(mid1);

            var root2 = new TestBehaviour2((c, t) => rootCalled[1] = true);
            var mid2 = new TestBehaviour2((c, t) =>
            {
                Assert.True(rootCalled[1]);
                midCalled[1] = true;
            }).Requires(root2);
            var dependant2 = new TestBehaviour2((c, t) =>
            {
                Assert.True(midCalled[1]);
                dependantCalled[1] = true;
            }).Requires(mid2);

            var independent1 = new TestBehaviour((c, t) => independentCalled[0] = true);
            var independent2 = new TestBehaviour2((c, t) => independentCalled[1] = true);

            RunBehaviourTest(new[] { dependant1, independent1, dependant2,
                mid2, mid1, root1, independent2, root2 },
                c =>
                {
                    Assert.True(dependantCalled[0]);
                    Assert.True(dependantCalled[1]);
                    Assert.True(independentCalled[0]);
                    Assert.True(independentCalled[1]);
                },
                () => {
                    rootCalled[0] = rootCalled[1] = false;
                    midCalled[0] = midCalled[1] = false;
                    dependantCalled[0] = dependantCalled[1] = false;
                    independentCalled[0] = independentCalled[1] = false;
                });
        }

        [Fact]
        public void ShouldThrowExceptionForDirectCyclicDependencies()
        {
            var coDependant1 = new TestBehaviour();
            var coDependant2 = new TestBehaviour();
            coDependant1.Requires(coDependant2);
            coDependant2.Requires(coDependant1);

            RunBehaviourTest(new[] { coDependant1, coDependant2 },
                c => Assert.Throws<InvalidOperationException>(() => c.Configure(null, null)), 
                dontConfigure: true);
        }

        [Fact]
        public void ShouldThrowExceptionForTransitiveCyclicDependencies()
        {
            var root = new TestBehaviour();
            var intermediate = new TestBehaviour();
            var dependant = new TestBehaviour();
            root.Requires(dependant);
            intermediate.Requires(root);
            dependant.Requires(intermediate);

            RunBehaviourTest(new[] { root, intermediate, dependant },
                c => Assert.Throws<InvalidOperationException>(() => c.Configure(null, null)),
                dontConfigure: true);
        }

        internal class TestBehaviourBase : DependantBase<IContainerBehaviour>, IContainerBehaviour
        {
            private readonly Action<IContainer, ITargetContainer> _callback;
            public TestBehaviourBase(Action<IContainer, ITargetContainer> callback = null)
            {
                _callback = callback;
            }

            public void Configure(IContainer container, ITargetContainer targets)
            {
                _callback?.Invoke(container, targets);
            }
        }

        internal class TestBehaviour2 : TestBehaviourBase
        {
            public TestBehaviour2(Action<IContainer, ITargetContainer> callback = null)
                : base(callback) { }
        }

        internal class TestBehaviour : TestBehaviourBase
        {
            public TestBehaviour(Action<IContainer, ITargetContainer> callback = null)
                : base(callback) { }
        }
    }
}
