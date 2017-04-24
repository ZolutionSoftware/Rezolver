using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{

    public class ContainerBehaviourCollectionTests
    {
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

        [Fact]
        public void ShouldConfigureDependencyBeforeDependantBehaviour()
        {
            // The IDependantBehaviour interface provides the ability for behaviours to declare 
            // a dependency on other behaviours.  The behaviours collection automatically processes
            // those dependencies when Configure() is called 

            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            bool rootCalled = false, dependantCalled = false;
            var root = new TestBehaviour((c, t) => rootCalled = true);
            var dependant = new TestBehaviour((c, t) =>
            {
                Assert.True(rootCalled);
                dependantCalled = true;
            }).Requires(root);

            behaviours.AddAll(dependant, root);
            behaviours.Configure(null, null);

            Assert.True(dependantCalled);
        }

        [Fact]
        public void ShouldConfigureTransitiveDependencyBeforeDependants()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
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

            behaviours.AddAll(dependant, mid, root);

            behaviours.Configure(null, null);
            Assert.True(dependantCalled);
        }

        [Fact]
        public void ShouldConfigureDependencyBeforeAllDependants()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            bool rootCalled = false;
            int dependantCalled = 0;
            var root = new TestBehaviour((c, t) => rootCalled = true);
            var dependantCallBack = new Action<IContainer, ITargetContainer>((c, t) =>
            {
                Assert.True(rootCalled);
                dependantCalled++;
            });

            behaviours.AddAll(root, 
                new TestBehaviour(dependantCallBack).Requires(root),
                new TestBehaviour(dependantCallBack).Requires(root),
                new TestBehaviour(dependantCallBack).Requires(root));

            behaviours.Configure(null, null);

            Assert.Equal(3, dependantCalled);
        }

        [Fact]
        public void ShouldConfigureDependantsWithIndependents()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
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

            behaviours.AddAll(dependant, independent, root);
            var ordered = behaviours.OrderByDependency().ToArray();
            behaviours.Configure(null, null);

            Assert.True(dependantCalled);
            Assert.True(independentCalled);
        }

        [Fact]
        public void ShouldConfigureDependantsOfTwoGroupsOfRelatedBehaviours()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();

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

            // try to start with worst case ordering
            behaviours.AddAll(dependant1, independent1, dependant2,
                mid2, mid1, dependant1, dependant2, root1, independent2, root2);

            behaviours.Configure(null, null);

            Assert.True(dependantCalled[0]);
            Assert.True(dependantCalled[1]);
            Assert.True(independentCalled[0]);
            Assert.True(independentCalled[1]);
        }

        [Fact]
        public void ShouldThrowExceptionForDirectCyclicDependencies()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var coDependant1 = new TestBehaviour();
            var coDependant2 = new TestBehaviour();
            coDependant1.Requires(coDependant2);
            coDependant2.Requires(coDependant1);

            behaviours.AddAll(coDependant1, coDependant2);

            Assert.Throws<InvalidOperationException>(() => behaviours.Configure(null, null));
        }

        [Fact]
        public void ShouldThrowExceptionForTransitiveCyclicDependencies()
        {
            ContainerBehaviourCollection behaviours = new ContainerBehaviourCollection();
            var root = new TestBehaviour();
            var intermediate = new TestBehaviour();
            var dependant = new TestBehaviour();
            root.Requires(dependant);
            intermediate.Requires(root);
            dependant.Requires(intermediate);
           
            behaviours.AddAll(root, intermediate, dependant);

            Assert.Throws<InvalidOperationException>(() => behaviours.Configure(null, null));
        }
    }

    internal class TestBehaviourBase : IContainerBehaviour
    {
        private readonly Action<IContainer, ITargetContainer> _callback;
        private List<IContainerBehaviour> _required;
        public TestBehaviourBase(Action<IContainer, ITargetContainer> callback = null)
        {
            _callback = callback;
            _required = new List<IContainerBehaviour>();
        }

        public void Configure(IContainer container, ITargetContainer targets)
        {
            _callback?.Invoke(container, targets);
        }

        public IEnumerable<IContainerBehaviour> GetDependencies(IEnumerable<IContainerBehaviour> behaviours)
        {
            List<IContainerBehaviour> tempRequired = new List<IContainerBehaviour>(_required);
            int index;
            foreach (var behaviour in behaviours)
            {
                index = tempRequired.IndexOf(behaviour);
                if (index >= 0)
                {
                    tempRequired.RemoveAt(index);
                    yield return behaviour;
                }
            }
            if (tempRequired.Count != 0)
                throw new InvalidOperationException("One or more required dependencies not present");
        }

        internal TestBehaviourBase Requires(IContainerBehaviour second)
        {
            _required.Add(second);
            return this;
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
