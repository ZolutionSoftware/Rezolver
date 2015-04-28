using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ObjectTargetClosedGenericTests
	{
		[TestMethod]
		public void ShouldSupportClosedGeneric()
		{
			ObjectTarget target = new ObjectTarget(new int[0], typeof(IEnumerable<int>));
			Assert.IsTrue(target.SupportsType(typeof(IEnumerable<int>)));
		}
	}
}
