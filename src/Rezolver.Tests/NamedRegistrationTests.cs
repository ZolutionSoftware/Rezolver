using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRegistrationTests
	{
		[TestMethod]
		public void ShouldSupportNamedRegistration()
		{
			IRezolveTarget target = new ObjectTarget("hello world");
			IRezolverBuilder builder = new RezolverBuilder();
			builder.Register(target, path: "name");
			var target2 = builder.Fetch(typeof (string), "name");
			Assert.AreSame(target, target2.DefaultTarget);

		}

		[TestMethod]
		public void ShouldSupportTwoNamedRegistrations()
		{
			IRezolveTarget target1 = new ObjectTarget("hello world");
			IRezolveTarget target2 = new ObjectTarget("hello universe");
			IRezolverBuilder builder = new RezolverBuilder();
			builder.Register(target1, path: "string1");
			builder.Register(target2, path: "string2");
			var target1B = builder.Fetch(typeof (string), name: "string1");
			var target2B = builder.Fetch(typeof (string), name: "string2");

			Assert.AreSame(target1, target1B.DefaultTarget);
			Assert.AreSame(target2, target2B.DefaultTarget);
			
		}

		[TestMethod]
		public void ShouldSupportHierarchicalNaming()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			INamedRezolverBuilder childBuilder = builder.GetNamedBuilder("parent.child", create: true);
			Assert.IsNotNull(childBuilder);
			Assert.AreEqual("child", childBuilder.Name);
			INamedRezolverBuilder parentBuilder = builder.GetNamedBuilder("parent", create: false);
			Assert.AreEqual("parent", parentBuilder.Name);
		}
	}
}
