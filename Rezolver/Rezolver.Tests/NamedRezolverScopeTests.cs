using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class NamedRezolverScopeTests
	{
		[TestMethod]
		public void ShouldSupportName()
		{
			INamedRezolverScope scope = new NamedRezolverScope(null, "name");
			Assert.AreEqual("name", scope.Name);

		}
	}
}
