using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
    public class ExpressionTargetTests : TestsBase
    {
		//tests integration of ExpressionTarget with the container and targetcontainer framework

		[Fact] void ShouldResolveAnInt()
		{
			var container = CreateContainer();
			container.RegisterExpression(() => 17);
			Assert.Equal(17, container.Resolve<int>());
		}
		
		[Fact]
		public void ShouldResolveAnObjectAndReturnAMethodCall()
		{
			//so this is one of the ultimate tests:
			//declare an expression which resolves an object and returns the result
			//of a method call as its service
			//Here, the resolve call should be translated to a RezolvedTarget for the same
			//type.
			var container = CreateContainer();
			container.RegisterType<ServiceWithFunctions>();
			container.RegisterExpression(() => Functions.Resolve<ServiceWithFunctions>().GetChild(5));

			var expectedIntValue = new ServiceWithFunctions().GetChild(5);
			var childResult = container.Resolve<ServiceChild>();
			Assert.Equal(expectedIntValue.Output, childResult.Output);
		}
    }
}
