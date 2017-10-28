using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public partial class TargetTypeSelectorTests
    {
        [Fact]
        public void Covariant_ShouldBeCorrectForFuncObjectDelegate()
        {
            // Arrange
            var targets = new TargetContainer();
            var funcTarget = Target.ForObject(new Func<string>(() => "Hello World")); // <-- expected behaviour is that the presence of this registration
                                                                      // will cause the Func<string> to be included in the search list.
            targets.Register(funcTarget);

            // Act
            var result = new TargetTypeSelector(typeof(Func<object>), targets).ToArray();

            LogActual(result);

            // Assert
            Assert.Equal(new[] { typeof(Func<object>), typeof(Func<string>), typeof(Func<>) }, result);
        }

        [Fact]
        public void Covariant_ShouldBeCorrectForFuncIEnumerableCharDelegate()
        {
            // Arrange
            var targets = new TargetContainer();
            var funcTarget = Target.ForObject(new Func<string>(() => "Hello World"));

            targets.Register(funcTarget);

            // Act
            var result = new TargetTypeSelector(typeof(Func<IEnumerable<char>>), targets).ToArray();

            LogActual(result);

            // Assert
            Assert.Equal(new[] {
                typeof(Func<IEnumerable<char>>),
                typeof(Func<string>),
                typeof(Func<>).MakeGenericType(typeof(IEnumerable<>)),
                typeof(Func<>)
            }, result);
        }
    }
}
