using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class NamedRegistrationTests
	{
		[Fact]
		public void ShouldSupportNamedRegistration()
		{
			IRezolveTarget target = new ObjectTarget("hello world");
			IRezolverBuilder builder = new RezolverBuilder();
			builder.Register(target, path: "name");
			var target2 = builder.Fetch(typeof(string), "name");
			Assert.Same(target, target2.DefaultTarget);

		}

		[Fact]
		public void ShouldSupportTwoNamedRegistrations()
		{
			IRezolveTarget target1 = new ObjectTarget("hello world");
			IRezolveTarget target2 = new ObjectTarget("hello universe");
			IRezolverBuilder builder = new RezolverBuilder();
			builder.Register(target1, path: "string1");
			builder.Register(target2, path: "string2");
			var target1B = builder.Fetch(typeof(string), name: "string1");
			var target2B = builder.Fetch(typeof(string), name: "string2");

			Assert.Same(target1, target1B.DefaultTarget);
			Assert.Same(target2, target2B.DefaultTarget);

		}

		[Fact]
		public void ShouldSupportHierarchicalNaming()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			INamedRezolverBuilder childBuilder = builder.GetNamedBuilder("parent.child", create: true);
			Assert.NotNull(childBuilder);
			Assert.Equal("child", childBuilder.Name);
			INamedRezolverBuilder parentBuilder = builder.GetNamedBuilder("parent", create: false);
			Assert.Equal("parent", parentBuilder.Name);
		}
	}
}
