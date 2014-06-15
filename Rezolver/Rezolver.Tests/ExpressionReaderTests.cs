using System.Diagnostics;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver
{
}

namespace Rezolver.Tests
{
	[TestClass]
	public class ExpressionReaderTests
	{
		[TestMethod]
		public void ShouldExtractMethodFromCallExpression()
		{
			MethodInfo mi = MethodCallExtractor.ExtractCalledMethod((object o) => o.GetHashCode());
			MethodInfo expected = typeof (object).GetMethod("GetHashCode");
			Assert.AreEqual(expected, mi);

		}
	}
}
