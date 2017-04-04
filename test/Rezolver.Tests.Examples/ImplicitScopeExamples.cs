using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ImplicitScopeExamples
    {
        [Fact]
        public void ShouldDisposeWhenScopeDisposed()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<DisposableType>();

            DisposableType result;
            using(var scope = container.CreateScope())
            {
                result = scope.Resolve<DisposableType>();
            }

            Assert.True(result.Disposed);
            // </example1>
        }
    }
}
