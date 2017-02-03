using Rezolver.Targets;
using Rezolver.Tests.Types;
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
		public void DefaultTarget_Int()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(int)));
			var container = CreateContainer(targets);
			Assert.Equal(default(int), container.Resolve<int>());

		}

		[Fact]
		public void DefaultTarget_NullableInt()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(int?)));
			var container = CreateContainer(targets);
			Assert.Equal(default(int?), container.Resolve<int?>());
		}

		[Fact]
		public void DefaultTarget_String()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(string)));
			var container = CreateContainer(targets);
			Assert.Equal(default(string), container.Resolve<string>());
		}

		[Fact]
		public void DefaultTarget_GenericRefType()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(List<int>)));
			var container = CreateContainer(targets);
			Assert.Equal(default(List<int>), container.Resolve<List<int>>());
		}

		[Fact]
		public void DefaultTarget_GenericValueType()
		{
			var targets = CreateTargetContainer();
			targets.Register(new DefaultTarget(typeof(GenericValueType<string>)));
			var container = CreateContainer(targets);
			Assert.Equal(default(GenericValueType<string>), container.Resolve<GenericValueType<string>>());
		}
	}
}
