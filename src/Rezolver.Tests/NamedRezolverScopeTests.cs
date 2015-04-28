using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRezolverScopeTests
	{
		[TestMethod]
		public void ShouldSupportName()
		{
			INamedRezolverBuilder builder = new NamedRezolverBuilder(Moq.Mock.Of<IRezolverBuilder>(), "name");
			Assert.AreEqual("name", builder.Name);
		}
	}
}
