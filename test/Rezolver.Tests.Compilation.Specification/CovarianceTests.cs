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
            var container = new Container();
            container.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassChild>>(result);
        }

        [Fact]
        public void Covariance_ShouldFavourMostRecent()
        {
            // Arrange
            var container = new Container();
            container.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();
            container.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassGrandchild>>(result);
        }

        [Fact]
        public void Covariance_ShouldFavourMostRecent_Reversed()
        {
            // Arrange
            var container = new Container();
            container.RegisterType<Covariant<BaseClassGrandchild>, ICovariant<BaseClassGrandchild>>();
            container.RegisterType<Covariant<BaseClassChild>, ICovariant<BaseClassChild>>();

            // Act
            var result = container.Resolve<ICovariant<BaseClass>>();

            // Assert
            Assert.IsType<Covariant<BaseClassChild>>(result);
        }
    }
}
