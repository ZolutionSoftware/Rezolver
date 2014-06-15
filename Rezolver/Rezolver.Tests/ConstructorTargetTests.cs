using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Tests
{
	[TestClass]
	public class ConstructorTargetTests : TestsBase
	{
		private class DefaultConstructor
		{
			
		}

		[TestMethod]
		public void ShouldAutomaticallyFindDefaultConstructor()
		{
			var target = new ConstructorTarget(typeof (DefaultConstructor));
			Assert.IsInstanceOfType(GetValueFromTarget(target), typeof(DefaultConstructor));
		}
	}
}
