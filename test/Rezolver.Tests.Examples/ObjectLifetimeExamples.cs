using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
    public class ObjectLifetimeExamples
    {
		[Fact]
        public void ShouldCreateInstanceOfMyService()
        {
			// <MyServiceDirect>
			Container container = new Container();
			container.RegisterType<MyService>();
			var result = container.Resolve<MyService>();
			Assert.NotNull(result);
			// </MyServiceDirect>
        }
    }
}
