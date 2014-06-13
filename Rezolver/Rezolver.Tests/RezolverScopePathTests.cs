using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolverScopePathTests
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
			Assert.AreEqual("parent", path.Current);
			Assert.IsTrue(path.MoveNext());
			Assert.AreEqual("child", path.Current);
			Assert.IsFalse(path.MoveNext());
		}

		[TestMethod]
		public void ShouldAllowPeekingNextPathItem()
		{
			RezolverScopePath path = new RezolverScopePath("parent.child.grandchild");
			//before walking starts, the next item should be the first
			Assert.AreEqual("parent", path.Next);
			path.MoveNext();
			Assert.AreEqual("child", path.Next);
			path.MoveNext();
			Assert.AreEqual("grandchild", path.Next);
			Assert.IsFalse(path.MoveNext());
			Assert.IsNull(path.Next);
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
