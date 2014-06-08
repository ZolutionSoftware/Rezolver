using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ChildRezolverTests
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void MustNotAllowNullParent()
		{
			IChildRezolverScope scope = new ChildRezolverScope(null);

		}

		[TestMethod]
		public void MustCopyParent()
		{
			var parent = new RezolverScope();
			IChildRezolverScope childScope = new ChildRezolverScope(parent);
			Assert.AreEqual(parent, childScope.ParentScope);
		}
	}
}
