using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	 [TestClass]
	 public class ObjectTargetTests
	 {
		  [TestMethod]
		  public void ShouldWrapNull()
		  {
				IRezolveTarget target = new ObjectTarget(null);
				Assert.IsNull(target.GetObject());
		  }

		  [TestMethod]
		  public void ShouldWrapNonNull()
		  {
				IRezolveTarget target = new ObjectTarget("Hello world");
				Assert.AreEqual("Hello world", target.GetObject());
		  }
	 }
}
