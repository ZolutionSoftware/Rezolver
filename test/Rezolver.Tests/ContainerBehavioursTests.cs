using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{

    public class ContainerBehavioursTests
    {
        [Fact]
        public void ShouldBeEmptyOnConstruction()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            Assert.Empty(behaviours);
        }

        [Fact]
        public void ShouldAddBehaviour()
        {
            // behaviours can be added and then should appear in the IEnumerable
            ContainerBehaviours behaviours = new ContainerBehaviours();
            var behaviour = new TestBehaviour();
            behaviours.Add(behaviour);
            Assert.Contains(behaviour, behaviours);
        }

        [Fact]
        public void ShouldAddAllBehaviours()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            var expected = new IContainerBehaviour[]
            {
                new TestBehaviour(), new TestBehaviour2()
            };

            behaviours.AddBehaviours(expected);

            Assert.Equal(expected, behaviours);
        }

        [Fact]
        public void ShouldAddAllBehavioursOnConstruction()
        {
            var expected = new IContainerBehaviour[]
            {
                new TestBehaviour(), new TestBehaviour2()
            };
            ContainerBehaviours behaviours = new ContainerBehaviours(expected);

            Assert.Equal(expected, behaviours);
        }

        [Fact]
        public void ShouldRemoveBehaviour()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            var behaviour = new TestBehaviour();
            behaviours.Add(behaviour);
            behaviours.Remove(behaviour);
            Assert.Empty(behaviours);
        }

        [Fact]
        public void ShouldRemoveBehaviours()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
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

            behaviours.AddBehaviours(toRemove);
            behaviours.AddBehaviours(remaining);
            behaviours.RemoveBehaviours(toRemove);

            Assert.Equal(remaining, behaviours);
        }

        [Fact]
        public void ShouldReplaceBehaviour()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            var original = new TestBehaviour();
            var replacement = new TestBehaviour2();

            behaviours.Add(original);
            behaviours.Replace(original, replacement);

            Assert.Equal(new[] { replacement }, behaviours);
        }

        [Fact]
        public void ShouldConfigureAddedBehaviour()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            bool configured = false;

            behaviours.Add(new TestBehaviour((c, t) => configured = true));

            behaviours.Configure(null, null);
            Assert.True(configured);
        }

        [Fact]
        public void ShouldConfigureAllAddedBehaviours()
        {
            ContainerBehaviours behaviours = new ContainerBehaviours();
            int configured = 0;

            behaviours.Add(new TestBehaviour((c, t) => configured++));
            behaviours.Add(new TestBehaviour((c, t) => configured++));
            behaviours.Add(new TestBehaviour((c, t) => configured++));

            behaviours.Configure(null, null);

            Assert.Equal(3, configured);
        }

        [Fact]
        public void ShouldConfigureBehavioursInOrder()
        {
            // behaviours must be applied in the order they're registered,
            // assuming no dependencies etc.
            ContainerBehaviours behaviours = new ContainerBehaviours();
            int firstConfigured = 0;
            int secondConfigured = 0;
            var counter = new Counter();
            behaviours.Add(new TestBehaviour((c, t) => firstConfigured = counter.Next()));
            behaviours.Add(new TestBehaviour((c, t) => secondConfigured = counter.Next()));

            behaviours.Configure(null, null);
            Assert.Equal(1, firstConfigured);
            Assert.Equal(2, secondConfigured);
        }

        [Fact]
        public void ShouldConfigureDependantBehaviourAfterDependency()
        {
            // The ContainerBehaviours type allows behaviours to declare a dependency on other behaviours,
            // specifying also whether that dependency is optional or required.  If a dependency is required, 
            // and not present, then the behaviour cannot be executed.  If it's present, then the behaviour will
            // run after that dependency has been run.  If it's an optional dependency and not present, then the
            // behaviour is free to be run at any time, and will most likely be executed last.

            ContainerBehaviours behaviours = new ContainerBehaviours();
            int firstConfigured = 0, secondConfigured = 0;
            var counter = new Counter();
            var requiresSecond = new TestBehaviour((c, t) => secondConfigured = counter.Next());
            var second = new TestBehaviour2((c, t) => firstConfigured = counter.Next());

            requiresSecond.Requires(second);

            behaviours.Configure(null, null);
            Assert.Equal(1, firstConfigured);
            Assert.Equal(2, secondConfigured);
        }
    }

    internal class Counter
    {
        private IEnumerator<int> _enumerator;

        public Counter()
        {
            _enumerator = Enumerable.Range(1, int.MaxValue).GetEnumerator();
        }

        public int Next()
        {
            _enumerator.MoveNext();
            return _enumerator.Current;
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
                throw new BehaviourConfigurationException("One or required dependencies not present");
        }

        internal TestBehaviourBase Requires(TestBehaviour2 second)
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
