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
        public void TypeBeingCreatedShouldBeSet()
        {
            var targets = CreateTargetContainer();
            targets.RegisterType<RequiresResolveContext>();
            targets.RegisterDelegate(rc => rc);
            var container = CreateContainer(targets);
            throw new NotImplementedException("This test isn't complete");
        }
    }
}
