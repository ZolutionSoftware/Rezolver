using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ChildRezolverBuilderTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullParent()
		{
			IChildRezolverBuilder builder = new ChildRezolverBuilder(null);

		}

		[TestMethod]
		public void MustCopyParent()
		{
			var parent = new RezolverBuilder();
			IChildRezolverBuilder childBuilder = new ChildRezolverBuilder(parent);
			Assert.AreEqual(parent, childBuilder.ParentBuilder);
		}
	}
}
