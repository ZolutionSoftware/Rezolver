using Rezolver.Tests.Examples.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Examples
{
	public class ConstructorExamples
	{

		[Fact]
		public void ShouldBuildRequiresMyService()
		{
			//<example1>
			var container = new Container();
			container.RegisterType<MyService>();
			container.RegisterType<RequiresMyService>();

			var result = container.Resolve<RequiresMyService>();

			Assert.NotNull(result.Service);
			//</example1>
		}

		[Fact]
		public void ShouldBuildRequiresMyServiceWithIMyService()
		{
			//<example2>
			var container = new Container();
			container.RegisterType<MyService, IMyService>();
			container.RegisterType<RequiresMyService>();

			var result = container.Resolve<RequiresMyService>();

			Assert.NotNull(result.Service);
			//</example2>
		}

		[Fact]
		public void ShouldRejectIncompatibleIMyService()
		{
			//<example3>
			var container = new Container();
			container.RegisterType<MyAlternateService, IMyService>();
			container.RegisterType<RequiresMyService>();
			//proves that the ConstructorTarget is selecting the constructor
			//based on the available services.
			Assert.Throws<ArgumentException>("service",
				() => container.Resolve<RequiresMyService>());
			//</example3>
		}
	}
}
