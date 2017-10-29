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
    }
}
