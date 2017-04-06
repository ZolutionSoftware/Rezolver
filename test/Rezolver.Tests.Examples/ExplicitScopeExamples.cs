using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ExplicitScopeExamples
    {
        [Fact]
        public void ScopeShouldCreateOnlyOneScopedObject()
        {
            // <example1>
            using (var container = new ScopedContainer())
            {
                container.RegisterScoped<MyService, IMyService>();

                var result1 = container.Resolve<IMyService>();
                var result2 = container.Resolve<IMyService>();

                Assert.Same(result1, result2);
            }
            // </example1>
        }

        [Fact]
        public void ScopeShouldCreateDifferentScopedObjectsFromItsParent()
        {
            // <example2>
            using(var container = new ScopedContainer())
            {
                container.RegisterScoped<MyService, IMyService>();
                var result1 = container.Resolve<IMyService>();
                IMyService result2;
                using(var scope = container.CreateScope())
                {
                    result2 = scope.Resolve<IMyService>();
                    Assert.NotSame(result1, result2);
                }
            }
            // </example2>
        }
    }
}
