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
        public void MixedVariance_ShouldGetMismatchedFunc()
        {
            // Arrange
            var targets = CreateTargetContainer();
            var del = new Func<object, string>(o => o.ToString());
            targets.RegisterObject(del);
            var container = CreateContainer(targets);

            // Act
            var result = container.Resolve<Func<string, object>>();

            // Assert
            Assert.Same(del, result);
        }
    }
}
