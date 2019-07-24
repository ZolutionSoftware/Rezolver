using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Rezolver.Sdk;

namespace Rezolver.Tests
{
    public class DependantCollectionTests
    {
        internal class TestDependant : Dependant
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



        internal class TestDependant1 : TestDependant
        {
            public TestDependant1(Action callback = null)
                : base(callback) { }
        }

        internal class TestDependant2 : TestDependant
        {
            public TestDependant2(Action callback = null)
                : base(callback) { }
        }

        internal class TestDependant3 : TestDependant
        {
            public TestDependant3(Action callback = null)
                : base(callback) { }
        }

        // For the dependency ordering tests.  Those are candidates for theories, clearly - but these will do for now
        // because some of these tests would generate thousands of theories.

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
                .Select(r =>
                {
                    return r.Select(i => baseArray[i]).ToArray();
                });
        }

        private void RunTest(TestDependant[] coll,
            Action<DependantCollection<TestDependant>> test = null,
            Action reset = null,
            bool dontConfigure = false)
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
                    if (!dontConfigure) Run(collection);
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

        ITestOutputHelper Output { get; }
        public DependantCollectionTests(ITestOutputHelper output)
        {
            Output = output;
        }

        private void Run(DependantCollection<TestDependant> coll)
        {
            // obtains the dependants in dependency order from the collection
            // and then executes their callbacks in order
            foreach (var dep in coll.Ordered)
            {
                dep.Run();
            }
        }

        [Fact]
        public void ShouldBeEmptyOnConstruction()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            Assert.Empty(coll);
        }

        [Fact]
        public void ShouldAddDependant()
        {
            // coll can be added and then should appear in the IEnumerable
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var dependant = new TestDependant1();
            coll.Add(dependant);
            Assert.Contains(dependant, coll);
        }

        [Fact]
        public void ShouldAddAllDependants()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            TestDependant[] expected = { new TestDependant1(), new TestDependant2() };

            coll.AddAll(expected);

            Assert.Equal(expected, coll);
        }

        [Fact]
        public void ShouldAddAllDependantsOnConstruction()
        {
            var expected = new TestDependant[]
            {
                new TestDependant1(), new TestDependant2()
            };
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>(expected);

            Assert.Equal(expected, coll);
        }

        [Fact]
        public void ShouldRemoveDependant()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var dependant = new TestDependant1();
            coll.Add(dependant);
            coll.Remove(dependant);
            Assert.Empty(coll);
        }

        [Fact]
        public void ShouldRemoveDependants()
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
        public void ShouldReplaceDependant()
        {
            DependantCollection<TestDependant> coll = new DependantCollection<TestDependant>();
            var original = new TestDependant1();
            var replacement = new TestDependant2();

            coll.Add(original);
            coll.Replace(original, replacement);

            Assert.Equal(new[] { replacement }, coll);
        }

        [Fact]
        public void ShouldOrderAllAddedDependants()
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
        public void ShouldOrderDependantsInRegistrationOrder()
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

        [Fact]
        public void ShouldOrderObjectDependencyBeforeDependant()
        {
            bool rootCalled = false, dependantCalled = false;
            var root = new TestDependant1(() => rootCalled = true);
            var dependant = new TestDependant2(() =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            }).Requires(root);

            RunTest(new TestDependant[] { root, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldOrderTypeDependencyBeforeDependant()
        {
            // similar to above, except the dependency is expressed as a type dependency
            bool rootCalled = false, dependantCalled = false;
            var root = new TestDependant1(() => rootCalled = true);
            var dependant = new TestDependant2(() =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            }).RequiresAny(typeof(TestDependant1));

            RunTest(new TestDependant[] { root, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldOrderTransitiveObjectDependencyBeforeDependants()
        {
            bool rootCalled = false, midCalled = false, dependantCalled = false;

            var root = new TestDependant1(() => rootCalled = true);
            var mid = new TestDependant2(() =>
            {
                Assert.True(rootCalled);
                midCalled = true;
            }).Requires(root);

            var dependant = new TestDependant3(() =>
            {
                Assert.True(midCalled);
                dependantCalled = true;
            }).Requires(mid);

            RunTest(new TestDependant[] { root, mid, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = midCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldOrderTransitiveTypeDependencyBeforeDependants()
        {
            // same as above, but this time the dependencies are expressed as
            // type dependencies.  Therefore dependant should inherit a dependency
            // on root via its type dependency on TestDependant2
            bool rootCalled = false, midCalled = false, dependantCalled = false;

            var root = new TestDependant1(() => rootCalled = true);
            var mid = new TestDependant2(() =>
            {
                Assert.True(rootCalled);
                midCalled = true;
            }).RequiresAny(typeof(TestDependant1));

            var dependant = new TestDependant3(() =>
            {
                Assert.True(midCalled);
                dependantCalled = true;
            }).RequiresAny(typeof(TestDependant2));

            RunTest(new TestDependant[] { root, mid, dependant },
                c => Assert.True(dependantCalled),
                () => rootCalled = midCalled = dependantCalled = false);
        }

        [Fact]
        public void ShouldOrderObjectDependencyBeforeAllDependants()
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
        public void ShouldOrderTypeDependencyBeforeAllDependants()
        {
            // should be getting used to this by now - same as above, but a Type dependency
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
                    new TestDependant2(dependantCallBack).RequiresAny(typeof(TestDependant1)),
                    new TestDependant2(dependantCallBack).RequiresAny(typeof(TestDependant1)),
                    new TestDependant2(dependantCallBack).RequiresAny(typeof(TestDependant1))
                },
                c => Assert.Equal(3, dependantCalled),
                () =>
                {
                    rootCalled = false;
                    dependantCalled = 0;
                });
        }

        [Fact]
        public void ShouldOrderDependantsWithIndependents()
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

            RunTest(new[] { dependant, independent, root },
                c =>
                {
                    Assert.True(dependantCalled);
                    Assert.True(independentCalled);
                },
                () => rootCalled = dependantCalled = independentCalled = false);
        }

        [Fact]
        public void ShouldOrderDependantsOfTwoGroupsOfRelatedDependants()
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

            RunTest(new TestDependant[]{ dependant1, independent1, dependant2,
                mid2, mid1, root1, independent2, root2 },
                c =>
                {
                    Assert.True(dependantCalled[0]);
                    Assert.True(dependantCalled[1]);
                    Assert.True(independentCalled[0]);
                    Assert.True(independentCalled[1]);
                },
                () =>
                {
                    rootCalled[0] = rootCalled[1] = false;
                    midCalled[0] = midCalled[1] = false;
                    dependantCalled[0] = dependantCalled[1] = false;
                    independentCalled[0] = independentCalled[1] = false;
                });
        }

        // Some more Type dependency tests

        [Fact]
        public void ShouldOrderTypeDependenciesBeforeDependantOfSameType()
        {
            // here we have a group of objects of the same type, with one having a type dependency
            // which matches all of them.  It should be executed last
            var dependenciesCalled = 0;
            var dependantCalled = false;

            var dependencies = Enumerable.Range(0, 3).Select(i => new TestDependant1(() => dependenciesCalled++));
            var dependant = new TestDependant1(() =>
            {
                Assert.Equal(3, dependenciesCalled);
                dependantCalled = true;
            }).RequiresAny(typeof(TestDependant1));

            RunTest(dependencies.Concat(new[] { dependant }).ToArray(),
                c => Assert.True(dependantCalled),
                () => {
                    dependenciesCalled = 0;
                    dependantCalled = false;
                });
        }

        [Fact]
        public void ShouldOrderTypeDependenciesBeforeMultipleDependantsOfSameType()
        {
            // similar to above - this time we have one object that should match the Type dependency,
            // and two dependants with identical type dependencies which also match themselves *and* each other
            // both will simply end up with a dependency on the one
            bool dependencyCalled = false, dependant1Called = false, dependant2Called = false;

            var dependency = new TestDependant1(() => dependencyCalled = true);
            var dependant1 = new TestDependant1(() =>
            {
                Assert.True(dependencyCalled);
                dependant1Called = true;
            }).RequiresAny(typeof(TestDependant1));
            var dependant2 = new TestDependant1(() =>
            {
                Assert.True(dependencyCalled);
                dependant2Called = true;
            }).RequiresAny(typeof(TestDependant1));

            RunTest(new[] { dependency, dependant1, dependant2 },
                c => Assert.True(dependant1Called && dependant2Called),
                () =>
                {
                    dependencyCalled = dependant1Called = dependant2Called = false;
                });
        }

        [Fact]
        public void ShouldOrderCombinedTypeAndObjectDependenciesBeforeMultipleDependantsOfSameType()
        {
            // builds on the previous two - one root dependency, and two objects of the same type 
            // both dependant on the same type.  One of those also has an explicit dependency on the
            // other, though, pushing it last
            bool rootCalled = false, midCalled = false, dependantCalled = false;
            var root = new TestDependant1(() => rootCalled = true);
            var mid = new TestDependant1(() => {
                Assert.True(rootCalled);
                midCalled = true;
            }).RequiresAny(typeof(TestDependant1));
            var dependant = new TestDependant1(() =>
            {
                Assert.True(midCalled);
                dependantCalled = true;
            }).RequiresAny(typeof(TestDependant1))
              .Requires(mid);

            RunTest(new[] { root, mid, dependant },
                c => Assert.True(dependantCalled),
                () =>
                {
                    rootCalled = midCalled = dependantCalled = false;
                });
        }

        [Fact]
        public void ShouldOrderAfterOptionalDependency()
        {
            // don't run the full suite of tests for optional dependencies, because all the previous tests
            // confirm that the ordering algorithm works.   The main thing is that if a dependency is optional,
            // then the system quite happily works when the dependency isn't there and when it is.

            bool? rootCalled = null, dependantCalled = false;

            var root = new TestDependant1(() => rootCalled = true);
            var dependant = new TestDependant1(() => {
                if(rootCalled.HasValue)
                    Assert.True(rootCalled);
                dependantCalled = true;
            }).After(root);

            //run the test twice: 

            //once with the dependency
            RunTest(new[] { root, dependant },
                c => Assert.True(dependantCalled),
                () =>
                {
                    rootCalled = null;
                    dependantCalled = false;
                });

            //once without (verifying that the optional dependency behaviour works)
            RunTest(new[] { dependant },
                c => {
                    Assert.Null(rootCalled);
                    Assert.True(dependantCalled);
                },
                () => dependantCalled = false);
        }

        [Fact]
        public void ShouldThrowExceptionForMissingDependency()
        {
            var required = new TestDependant1();
            var dependant = new TestDependant1().Requires(required);

            var coll = new DependantCollection<TestDependant>(new[] { dependant });

            Assert.Throws<DependencyException>(() => Run(coll));
        }

        [Fact]
        public void ShouldThrowExceptionForMissingTypeDependency()
        {
            var dependant = new TestDependant1().RequiresAny(typeof(TestDependant2));

            var coll = new DependantCollection<TestDependant>(new[] { dependant });

            Assert.Throws<DependencyException>(() => Run(coll));
        }

        [Fact]
        public void ShouldThrowExceptionForMissingTypeDependencyWhichMatchesSelf()
        {
            // similar to above, but here the dependency is declared as being on a type
            // which the dependant satisfies itself.  This should be ignored and an error
            // should still occur becausea dependency declared like this means 'there must be
            // *other* objects of the same type as me in here'.  
            var dependant = new TestDependant1().RequiresAny(typeof(TestDependant));

            var coll = new DependantCollection<TestDependant>(new[] { dependant });

            Assert.Throws<DependencyException>(() => Run(coll));
        }

        [Fact]
        public void ShouldThrowExceptionForTwoDependantsWithIdenticalTypeDependencies()
        {
            var dependant1 = new TestDependant1().RequiresAny(typeof(TestDependant));
            var dependant2 = new TestDependant2().RequiresAny(typeof(TestDependant));

            var coll = new DependantCollection<TestDependant>(new TestDependant[] { dependant1, dependant2 });

            var ioex = Assert.Throws<InvalidOperationException>(() => Run(coll));
            Assert.IsType<DependencyException>(ioex.InnerException);
        }

        [Fact]
        public void ShouldThrowExceptionForDirectCyclicDependencies()
        {
            var coDependant1 = new TestDependant1();
            var coDependant2 = new TestDependant1();
            coDependant1.Requires(coDependant2);
            coDependant2.Requires(coDependant1);

            RunTest(new[] { coDependant1, coDependant2 },
                c => {
                    var ioex = Assert.Throws<InvalidOperationException>(() => Run(c));
                    Assert.IsType<DependencyException>(ioex.InnerException);
                },
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
            RunTest(new[] { root, intermediate, dependant },
                c => {
                    var ioex = Assert.Throws<InvalidOperationException>(() => Run(c));
                    Assert.IsType<DependencyException>(ioex.InnerException);
                },
                dontConfigure: true);
        }
    }
}
