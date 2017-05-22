using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class TargetContainerConfigurationTests
    {
        [Fact]
        public void Configuration()
        {
            var targets = new TargetContainer();

            targets.SetOption<AllowMultiple>(false)
                .SetOption<int, AllowMultiple>(true);

            //bool allowMultiple = targets.
        }


    }
}
