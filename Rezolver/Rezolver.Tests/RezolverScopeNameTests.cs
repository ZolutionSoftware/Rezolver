using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolverScopeNameTests
	{
		[TestMethod]
		public void ShouldCreateFromString()
		{
			RezolverScopePath path = new RezolverScopePath("parent");
			Assert.AreEqual("parent", path.Path);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullPath()
		{
			RezolverScopePath path = new RezolverScopePath(null);
		}

		[TestMethod]
		public void ShouldCreateMultiStepPathFromString()
		{
			RezolverScopePath path = new RezolverScopePath("parent.child");
			Assert.AreEqual("parent.child", path.Path);
			Assert.IsTrue(path.MoveNext());
			Assert.AreEqual("parent", path.CurrentItem);
			Assert.IsTrue("parent.child".Split('.').SequenceEqual(path));
		}

		[TestMethod]
		public void ShouldNotAllowAnyWhitespace()
		{
			var testArgs = new[]
			{
				" ",
				" a",
				"a ",
				"a. ",
				"a. b",
				"a. .b",
			};

			AssertEx.Throws<ArgumentException>().ForEach(testArgs, s => new RezolverScopePath(s));

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotAllowDoubleSeparator()
		{
			RezolverScopePath path = new RezolverScopePath("a..b");

		}


	}
}
