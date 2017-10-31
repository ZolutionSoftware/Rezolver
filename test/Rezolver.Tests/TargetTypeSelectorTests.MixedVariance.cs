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
        public void MixedVariance_ShouldMixContravarianceWithCovariance()
        {
            // Arrange
            TargetContainer targets = new TargetContainer();
            targets.RegisterObject<Func<BaseClass, BaseClassChild>>(bc => new BaseClassChild());

            // Act
            var result = new TargetTypeSelector(typeof(Func<BaseClassChild, BaseClass>), targets).ToArray();
            LogActual(result);

            // Assert
            Assert.Equal(new[]
            {
                typeof(Func<BaseClassChild, BaseClass>),
                typeof(Func<BaseClassChild, BaseClassChild>),
                typeof(Func<BaseClass, BaseClass>),
                typeof(Func<BaseClass, BaseClassChild>),
                typeof(Func<object, BaseClass>),
                typeof(Func<object, BaseClassChild>),
                typeof(Func<,>)
            },
            result);
        }
    }
}
