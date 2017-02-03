using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Compilation.Specification
{
	public partial class CompilerTestsBase
	{
		[Fact]
		public void ShouldReturnDefaultInt()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(int)));
			var container = CreateContainer(targets);
			Assert.Equal(default(int), container.Resolve<int>());

		}

		[Fact]
		public void ShouldReturnDefaultNullableInt()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(int?)));
			var container = CreateContainer(targets);
			Assert.Equal(default(int?), container.Resolve<int?>());
		}

		[Fact]
		public void ShouldReturnDefaultReferenceType_String()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(string)));
			var container = CreateContainer(targets);
			Assert.Equal(default(string), container.Resolve<string>());
		}

		[Fact]
		public void ShouldReturnDefaultReferenceType_Generic()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(List<int>)));
			var container = CreateContainer(targets);
			Assert.Equal(default(List<int>), container.Resolve<List<int>>());
		}
	}
}
