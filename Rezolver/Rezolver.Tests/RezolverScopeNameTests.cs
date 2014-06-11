using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolverScopeNameTests
	{
		[TestMethod]
		public void ShouldCreateFromString()
		{
			RezolverScopeName name = new RezolverScopeName("parent");
			Assert.AreEqual("parent", name.Path);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullPath()
		{
			RezolverScopeName name = new RezolverScopeName(null);

		}
	}
}
