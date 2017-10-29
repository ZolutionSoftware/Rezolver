using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public partial class TargetContainerTests
    {
        public static TheoryData<string, Type, Type> CovariantTypeData = new TheoryData<string, Type, Type>
        {
            { "Func<string> -> Func<object>", typeof(Func<string>), typeof(Func<object>) },
            { "Func<string> -> Func<IEnumerable<char>>", typeof(Func<string>), typeof(Func<IEnumerable<char>>) },
            { "ICovariant<BaseClassGrandchild> -> ICovariant<BaseClass>", typeof(Func<BaseClassGrandchild>), typeof(Func<BaseClass>) }
        };

        [Theory]
        [MemberData(nameof(CovariantTypeData))]
        public void Covariant_ShouldFetch(string name, Type tTarget, Type toFetch)
        {
            // Arrange
            ITargetContainer targets = new TargetContainer();
            var target = new TestTarget(tTarget, false, true, ScopeBehaviour.None);
            targets.Register(target);

            // Act
            var fetched = targets.Fetch(toFetch);

            // Assert
            Assert.Same(target, fetched);
        }

        [Fact]
        public void Covariant_ShouldNotRetrieveConstrained()
        {
            // Arrange
            var targets = new TargetContainer();
            var expected = Target.ForType(typeof(Covariant<>));
            var notExpected = Target.ForType(typeof(ConstrainedCovariant<>));
            targets.Register(expected, typeof(ICovariant<>));
            targets.Register(notExpected, typeof(ICovariant<>));

            // Act
            var single = targets.Fetch(typeof(ICovariant<string>));
            var all = targets.FetchAll(typeof(ICovariant<string>));

            // Assert
            Assert.Same(expected, single);
            Assert.Single(all, expected);
        }

        [Fact]
        public void Covariant_ShouldFetchAllMatches()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.RegisterType<Covariant<BaseClass>, ICovariant<BaseClass>>();
            targets.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            targets.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();

            // Act
            var fetched = targets.FetchAll(typeof(ICovariant<BaseClass>)).ToArray();

            // Assert
            Assert.Equal(3, fetched.Length);
        }

        [Fact]
        public void Covariant_Enumerable_ShouldContainAllMatches()
        {
            // Arrange
            var targets = new TargetContainer();
            var baseTarget = Target.ForType<BaseClass>();
            var childTarget = Target.ForType<BaseClassChild>();
            var grandChildTarget = Target.ForType<BaseClassGrandchild>();
            targets.Register(baseTarget);
            targets.Register(childTarget);
            targets.Register(grandChildTarget);

            // Act
            var enumerableTarget = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<BaseClass>)));

            // Assert
            Assert.Equal(new[] { baseTarget, childTarget, grandChildTarget }, enumerableTarget.Targets);
        }

        [Fact]
        public void Covariant_Enumerable_ShouldContainAllMatches_NestedCovariant()
        {
            // Arrange
            var targets = new TargetContainer();
            var baseTarget = Target.ForType<Covariant<BaseClass>>();
            var childTarget = Target.ForType<Covariant<BaseClassChild>>();
            var grandChildTarget = Target.ForType<Covariant<BaseClassGrandchild>>();
            targets.Register(baseTarget, typeof(ICovariant<BaseClass>));
            targets.Register(childTarget, typeof(ICovariant<BaseClassChild>));
            targets.Register(grandChildTarget, typeof(ICovariant<BaseClassGrandchild>));

            // Act
            var enumerableTarget = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<ICovariant<BaseClass>>)));

            // Assert
            Assert.Equal(new[] { baseTarget, childTarget, grandChildTarget }, enumerableTarget.Targets);
        }

        [Fact]
        public void Covariant_Enumerable_ShouldContainOneMatchBecauseOptionDisablesEnumerableCovariance()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.SetOption<Options.EnableEnumerableCovariance>(false);
            var baseTarget = Target.ForType<BaseClass>();
            var childTarget = Target.ForType<BaseClassChild>();
            var grandChildTarget = Target.ForType<BaseClassGrandchild>();
            targets.Register(baseTarget);
            targets.Register(childTarget);
            targets.Register(grandChildTarget);

            // Act
            var enumerableTarget = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<BaseClass>)));

            // Assert
            Assert.Equal(new[] { baseTarget }, enumerableTarget.Targets);
        }

        [Fact]
        public void Covariant_Enumerable_ShouldContainOneMatchBecauseOptionDisablesEnumerableCovarianceForThatEnumerable()
        {
            // Arrange
            var targets = new TargetContainer();
            targets.SetOption<Options.EnableEnumerableCovariance, BaseClass>(false);
            var baseTarget = Target.ForType<BaseClass>();
            var childTarget = Target.ForType<BaseClassChild>();
            var grandChildTarget = Target.ForType<BaseClassGrandchild>();
            var nestedbaseTarget = Target.ForType<Covariant<BaseClass>>();
            var nestedchildTarget = Target.ForType<Covariant<BaseClassChild>>();
            var nestedgrandChildTarget = Target.ForType<Covariant<BaseClassGrandchild>>();
            targets.Register(baseTarget);
            targets.Register(childTarget);
            targets.Register(grandChildTarget);
            targets.Register(nestedbaseTarget, typeof(ICovariant<BaseClass>));
            targets.Register(nestedchildTarget, typeof(ICovariant<BaseClassChild>));
            targets.Register(nestedgrandChildTarget, typeof(ICovariant<BaseClassGrandchild>));

            // Act
            var enumerableTarget = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<BaseClass>)));
            var nestedEnumerableTarget = Assert.IsType<EnumerableTarget>(targets.Fetch(typeof(IEnumerable<ICovariant<BaseClass>>)));

            // Assert
            Assert.Equal(new[] { baseTarget }, enumerableTarget.Targets);
            Assert.Equal(new[] { nestedbaseTarget, nestedchildTarget, nestedgrandChildTarget }, nestedEnumerableTarget.Targets);
        }
    }
}
