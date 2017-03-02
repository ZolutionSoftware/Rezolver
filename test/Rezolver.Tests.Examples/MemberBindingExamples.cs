using Rezolver.Targets;
using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class MemberBindingExamples
	{

		[Fact]
		public void ShouldInject2MembersWithAllMembersBehaviour()
		{
			//<example1>
			var container = new Container();
			container.RegisterAll(
				ConstructorTarget.Auto<MyService1>(),
				ConstructorTarget.Auto<MyService2>()
			);
			container.RegisterType<Has2InjectableMembers>(DefaultMemberBindingBehaviour.Instance);

			var result = container.Resolve<Has2InjectableMembers>();

			Assert.NotNull(result.Service1);
			Assert.NotNull(result.Service2);
			//</example1>
		}
	}
}
