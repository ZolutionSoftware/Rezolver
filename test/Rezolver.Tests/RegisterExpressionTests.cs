using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	//Technically a suite of integration tests, but the individual components here all have unit tests
	//too, and the RegisterExpression extension methods are a unique integration of the TargetAdapter
	//and the TargetContainer
	//Lambdas have their own curiosities, too - in that their parameters have to be rewritten as local 
	//variable assignments from the container

	public class RegisterExpressionTests : TestsBase
	{
		[Fact]
		public void ShouldResolveAnInt()
		{
			var container = CreateContainer();
			container.RegisterExpression(() => 17);
			Assert.Equal(17, container.Resolve<int>());
		}

		[Fact]
		public void ShouldResolveAnIntFromArithmetic()
		{
			var container = CreateContainer();
			container.RegisterExpression(() => (34 * 20) / 4);
			Assert.Equal(170, container.Resolve<int>());
		}

		[Fact]
		public void ShouldReturnResolveContext()
		{
			//tests that the lambda's RezolveContext parameter gets correctly mapped to the
			//RezolveContext that is passed directly to the container.  Test is only possible
			//here because it's a direct resolve, i.e. caller -> container -> target
			var container = CreateContainer();
			//note that the single parameter overload of the RegisterExpression extension methods
			//default to the concrete RezolveContext versions.
			container.RegisterExpression(rc => rc);
			RezolveContext context = new RezolveContext(container, typeof(RezolveContext));
			Assert.Same(context, container.Resolve(context));
		}

		[Fact]
		public void ShouldInjectArgumentAndResolveMethodCall()
		{
			//although this looks the same as the RegisterDelegate, it's not, because of what happens to 
			//lambda expressions to make argument injection work.  There were two choices with additional
			//lambda arguments: compile the lambda into a delegate and then just use it like a delegate target,
			//or rewrite the lambda so that it could be 'imported' into other target expressions just like
			//the rest.  We chose the latter.  So a lambda with arguments actually gets rewritten into an 
			//expression with a block of local variables which are explicitly resolved in at the start
			//with the necessary services - i.e. if a lambda starts off:
			// (IService myservice) => myservice.MethodCall();
			//It will be rewritten to:
			// () => {
			//   IService myservice = Functions.Resolve<IService>();
			//	 return myservice.MethodCall();
			// }
			//The resolve op will also, of course, be rewritten to the correct expression for IService.
			var container = CreateContainer();
			container.RegisterType<ServiceWithFunctions>();
			container.RegisterExpression((ServiceWithFunctions s) => s.GetChild(4));
			Assert.Equal(new ServiceWithFunctions().GetChild(4).Output, container.Resolve<ServiceChild>().Output);
		}

		[Fact]
		public void ShouldResolveAnObjectAndReturnAMethodCall()
		{
			//When you want to explicitly resolve something in an expression, but don't want to or can't 
			//use a lambda argument, you can also use the Function.Resolve helper method.  It does nothing
			//at run time (except throw an exception) but it instructs the target adapter to replace that
			//call with a ResolvedTarget
			var container = CreateContainer();
			container.RegisterType<ServiceWithFunctions>();
			container.RegisterExpression(() => Functions.Resolve<ServiceWithFunctions>().GetChild(5));
			
			Assert.Equal(new ServiceWithFunctions().GetChild(5).Output, container.Resolve<ServiceChild>().Output);
		}
	}
}
