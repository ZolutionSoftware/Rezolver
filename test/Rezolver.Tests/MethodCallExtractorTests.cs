using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class MethodCallExtractorTests
	{
		//the class tested here is a key part of some of the core code for Rezolver, if not specifically used for the core functionality.
		//it's a time-saver, used to simplify some of the code written to support the main functionality, so it's important it works!
		[Fact]
		public void ShouldExtractMethodFromCallExpression()
		{
			MethodInfo mi = MethodCallExtractor.ExtractCalledMethod((object o) => o.GetHashCode());
			MethodInfo expected = typeof(object).GetMethod("GetHashCode");
			Assert.Equal(expected, mi);
		}
	}
}
