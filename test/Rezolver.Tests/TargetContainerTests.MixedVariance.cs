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
        [Fact]
        public void MixedVariance_ShouldFetchCompatibleFunc()
        {
            // Arrange
            var targets = new TargetContainer();
            var expected = Target.ForObject(new Func<object, string>(o => o.ToString()));
            targets.Register(expected);

            // Act
            var result = targets.Fetch(typeof(Func<string, object>));

            // Assert
            Assert.Equal(expected.Id, result.Id);
        }
    }
}
