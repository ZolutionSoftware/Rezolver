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
        internal class TestDependant : DependantBase<TestDependant>
        {
            private readonly Action _callback;
            public TestDependant(Action callback = null)
            {
                _callback = callback;
            }

            public void Run()
            {
                _callback?.Invoke();
            }
        }

        internal class TestDependant2 : TestDependant
        {
            public TestDependant2(Action callback = null)
                : base(callback) { }
        }

        internal class TestDependant1 : TestDependant
        {
            public TestDependant1(Action callback = null)
                : base(callback) { }
        }

        ITestOutputHelper Output { get; }
        public ContainerBehaviourCollectionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private void Run(DependantCollection<TestDependant> coll)
        {
            // obtains the dependants in dependency order from the collection
            // and then executes their callbacks in order
            foreach(var dep in coll.OrderByDependency())
            {
                dep.Run();
            }
        }

        // Note - this test also tests the lower-level DependantCollection as everything we're
        // doing here relies on that.  The DependantCollection<TestDependant> class itself is just a couple of
        // connstructors and a stub implementation of another which always returns an empty enumerable.

        [Fact]
        public void ShouldBeEmptyOnConstruction()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            Assert.Empty(coll);
        }

        [Fact]
        public void ShouldAddBehaviour()
        {
            // coll can be added and then should appear in the IEnumerable
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var behaviour = new TestDependant1();
            coll.Add(behaviour);
            Assert.Contains(behaviour, coll);
        }

        [Fact]
        public void ShouldAddAllBehaviours()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            TestDependant[] expected = { new TestDependant1(), new TestDependant2() };

            coll.AddAll(expected);

            Assert.Equal(expected, coll);
        }

        [Fact]
        public void ShouldAddAllBehavioursOnConstruction()
        {
            var expected = new TestDependant[]
            {
                new TestDependant1(), new TestDependant2()
            };
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>(expected);

            Assert.Equal(expected, coll);
        }

        [Fact]
        public void ShouldRemoveBehaviour()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var behaviour = new TestDependant1();
            coll.Add(behaviour);
            coll.Remove(behaviour);
            Assert.Empty(coll);
        }

        [Fact]
        public void ShouldRemoveBehaviours()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var toRemove = new TestDependant[]
            {
                new TestDependant1(),
                new TestDependant1(),
                new TestDependant1()
            };
            var remaining = new TestDependant[]
            {
                new TestDependant2()
            };

            coll.AddAll(toRemove);
            coll.AddAll(remaining);
            coll.RemoveAll(toRemove);

            Assert.Equal(remaining, coll);
        }

        [Fact]
        public void ShouldReplaceBehaviour()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var original = new TestDependant1();
            var replacement = new TestDependant2();

            coll.Add(original);
            coll.Replace(original, replacement);

            Assert.Equal(new[] { replacement }, coll);
        }

        [Fact]
        public void ShouldConfigureAddedBehaviour()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            bool configured = false;

            coll.Add(new TestDependant1(() => configured = true));

            Run(coll);
            Assert.True(configured);
        }

        [Fact]
        public void ShouldConfigureAllAddedBehaviours()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            int configured = 0;

            coll.Add(new TestDependant1(() => configured++));
            coll.Add(new TestDependant1(() => configured++));
            coll.Add(new TestDependant1(() => configured++));

            Run(coll);

            Assert.Equal(3, configured);
        }

        [Fact]
        public void ShouldConfigureBehavioursInRegistrationOrder()
        {
            // coll must be applied in the order they're registered,
            // assuming no dependencies etc.
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            bool firstCalled = true;
            bool secondCalled = true;

            coll.Add(new TestDependant1(() => firstCalled = true));
            coll.Add(new TestDependant1(() =>
            {
                Assert.True(firstCalled);
                secondCalled = true;
            }));

            Run(coll);
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

        private void RunTest(TestDependant[] coll, Action<DependantCollection<TestDependant>> test = null, Action reset = null, bool dontConfigure=false)
        {
            // runs the test for each unique permutation of the coll.
            // note that the test fails as soon as one order fails - and, because
            // I'm not proficient enouigh with Xunit yet - I'm just outputting the 
            // indices of the items, in order, which led to the failure.
            // Remember - keep the number of input items to a reasonable amount; with a
            // suggested maximum of 9-10 as the number of permutations is (n!) where n is the 
            // length of the coll array.
            int count = 0;
            foreach (var uniqueOrder in UniquePermutations(coll))
            {
                count++;
                var collection = new DependantCollection<TestDependant>(uniqueOrder);
                try
                {
                    if(!dontConfigure) Run(collection);
                    test?.Invoke(collection);
                    reset?.Invoke();
                }
                catch (Exception)
                {
                    Output.WriteLine($"Ordering of failed items: { string.Join(", ", uniqueOrder.Select(i => Array.IndexOf(coll, i))) } ");
                    throw;
                }
            }

            Output.WriteLine($"Total number of permutations run: { count }");
        }

        [Fact]
        public void ShouldConfigureDependencyBeforeDependantBehaviour()
        {
            // The IDependantBehaviour interface provides the ability for coll to declare 
            // a dependency on other coll.  The coll collection automatically processes
            // those dependencies when Configure() is called 
            bool rootCalled = false, dependantCalled = false;
            var root = new TestDependant1(() => rootCalled = true);
            var dependant = new TestDependant1(() =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            });
            dependant.Requires(root);

            RunTest(new TestDependant[] { root, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldConfigureTransitiveDependencyBeforeDependants()
        {
            bool rootCalled = false, midCalled = false, dependantCalled = false;

            var root = new TestDependant1(() => rootCalled = true);
            var mid = new TestDependant1(() =>
            {
                Assert.True(rootCalled);
                midCalled = true;
            }).Requires(root);

            var dependant = new TestDependant1(() =>
            {
                Assert.True(midCalled);
                dependantCalled = true;
            }).Requires(mid);

            RunTest(new TestDependant[] { root, mid, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = midCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldConfigureDependencyBeforeAllDependants()
        {
            bool rootCalled = false;
            int dependantCalled = 0;
            var root = new TestDependant1(() => rootCalled = true);
            var dependantCallBack = new Action(() =>
            {
                Assert.True(rootCalled);
                dependantCalled++;
            });

            RunTest(new TestDependant[] {
                    root,
                    new TestDependant1(dependantCallBack).Requires(root),
                    new TestDependant1(dependantCallBack).Requires(root),
                    new TestDependant1(dependantCallBack).Requires(root)
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
            var root = new TestDependant1(() => rootCalled = true);
            var dependant = new TestDependant1(() =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            }).Requires(root);
            var independent = new TestDependant1(() => independentCalled = true);

            RunTest(new IDependant<TestDependant>[] { dependant, independent, root },
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
            var root1 = new TestDependant1(() => rootCalled[0] = true);
            var mid1 = new TestDependant1(() =>
            {
                Assert.True(rootCalled[0]);
                midCalled[0] = true;
            }).Requires(root1);
            var dependant1 = new TestDependant1(() =>
            {
                Assert.True(midCalled[0]);
                dependantCalled[0] = true;
            }).Requires(mid1);

            var root2 = new TestDependant2(() => rootCalled[1] = true);
            var mid2 = new TestDependant2(() =>
            {
                Assert.True(rootCalled[1]);
                midCalled[1] = true;
            }).Requires(root2);
            var dependant2 = new TestDependant2(() =>
            {
                Assert.True(midCalled[1]);
                dependantCalled[1] = true;
            }).Requires(mid2);

            var independent1 = new TestDependant1(() => independentCalled[0] = true);
            var independent2 = new TestDependant2(() => independentCalled[1] = true);

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
            var coDependant1 = new TestDependant1();
            var coDependant2 = new TestDependant1();
            coDependant1.Requires(coDependant2);
            coDependant2.Requires(coDependant1);

            RunBehaviourTest(new[] { coDependant1, coDependant2 },
                c => Assert.Throws<InvalidOperationException>(() => c.Run()), 
                dontConfigure: true);
        }

        [Fact]
        public void ShouldThrowExceptionForTransitiveCyclicDependencies()
        {
            var root = new TestDependant1();
            var intermediate = new TestDependant1();
            var dependant = new TestDependant1();
            root.Requires(dependant);
            intermediate.Requires(root);
            dependant.Requires(intermediate);
            RunBehaviourTest(new[] { root, intermediate, dependant },
                c => Assert.Throws<InvalidOperationException>(() => c.Run()),
                dontConfigure: true);
        }
    }
}
