using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class QuickStartExamples
    {
        internal class MyType
        {

        }

        [Fact]
        public void Simple()
        {
            // <example1>
            var container = new Container();
            container.RegisterType<MyType>();

            var instance = container.Resolve<MyType>();
            // </example1>

            Assert.NotNull(instance);
        }
    }
}
