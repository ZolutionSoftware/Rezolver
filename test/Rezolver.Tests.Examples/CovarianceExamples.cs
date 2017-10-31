using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class CovarianceExamples
    {
        [Fact]
        public void ShouldFetchCovariantMatch()
        {
            // <example1>
            var container = new Container();
            container.RegisterObject<Func<MyService2>>(() => new MyService2());

            var result = container.Resolve<Func<IMyService>>();

            Assert.IsType<Func<MyService2>>(result);
            // </example1>
        }
    }
}
