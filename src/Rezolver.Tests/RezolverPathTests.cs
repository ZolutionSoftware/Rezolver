using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolverPathTests
	{
		[TestMethod]
		public void ShouldCreateFromString()
		{
			RezolverPath path = new RezolverPath("parent");
			Assert.AreEqual("parent", path.Path);
		}

		[TestMethod]
		public void ShouldCreateFromStringImplicit()
		{
			RezolverPath path = "parent.child";
			Assert.AreEqual("parent.child", path.Path);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullPath()
		{
			RezolverPath path = new RezolverPath(null);
		}

		[TestMethod]
		public void ShouldCreateMultiStepPathFromString()
		{
			RezolverPath path = new RezolverPath("parent.child");
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
			RezolverPath path = new RezolverPath("parent.child.grandchild");
			//before walking starts, the next item should be the first
			Assert.AreEqual("parent", path.Next);
			path.MoveNext();
			Assert.AreEqual("child", path.Next);
			path.MoveNext();
			Assert.AreEqual("grandchild", path.Next);
			Assert.IsTrue(path.MoveNext());
			Assert.IsNull(path.Next);
			Assert.IsFalse(path.MoveNext());

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

			AssertEx.Throws<ArgumentException>().ForEach(testArgs, s => new RezolverPath(s));

		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotAllowDoubleSeparator()
		{
			RezolverPath path = new RezolverPath("a..b");
		}

	}
}
