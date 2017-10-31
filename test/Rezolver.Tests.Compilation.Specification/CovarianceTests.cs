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
        public void Covariance_ShouldResolveGenericWithDerivedClass()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassChild>>(result);
        }

        [Fact]
        public void Covariance_ShouldFavourMostRecent()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            targets.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassGrandchild>>(result);
        }

        [Fact]
        public void Covariance_ShouldFavourMostRecent_Reversed()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();
            targets.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassChild>>(result);
        }

        [Fact]
        public void Covariance_EnumerableShouldIncludeAllMatches()
        {
            // Arrange
            var targets = CreateTargetContainer();
            targets.RegisterType<Covariant<BaseClass>, ICovariant<BaseClass>>();
            targets.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            targets.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();
            var container = CreateContainer(targets);

            // Act
            var result = container.ResolveMany<ICovariant<BaseClass>>();

            // Assert
            // note that this tests that registration order is honoured
            Assert.Collection(result, new Action<ICovariant<BaseClass>>[]
            {
                e => Assert.IsType<Covariant<BaseClass>>(e),
                e => Assert.IsType<Covariant<BaseClassChild>>(e),
                e => Assert.IsType<Covariant<BaseClassGrandchild>>(e)
            });
        }
    }
}
