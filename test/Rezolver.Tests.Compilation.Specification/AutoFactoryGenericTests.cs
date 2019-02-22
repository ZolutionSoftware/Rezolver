using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
    public partial class CompilerTestsBase
    {
        [Fact]
        public void AutoFactory_ShouldCreateGeneric()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType(typeof(Generic<>));
            targets.RegisterAutoFactory(typeof(Func<>).MakeGenericType(typeof(Generic<>)));
            var container = CreateContainer(targets);

            // Act
            var func = container.Resolve<Func<Generic<int>>>();
            var result = func();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void AutoFactory_ShouldCreateGenericViaInterface()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
            targets.RegisterAutoFactory(typeof(Func<>).MakeGenericType(typeof(IGeneric<>)));
            var container = CreateContainer(targets);

            // Act4
            var func = container.Resolve<Func<IGeneric<int>>>();
            var result = func();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Generic<int>>(result);
        }
    }
}
