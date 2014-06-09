using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRezolverScopeTests
	{
		[TestMethod]
		public void ShouldSupportName()
		{
			INamedRezolverScope scope = new NamedRezolverScope(Moq.Mock.Of<IRezolverScope>(), "name");
			Assert.AreEqual("name", scope.Name);
		}
	}
}
