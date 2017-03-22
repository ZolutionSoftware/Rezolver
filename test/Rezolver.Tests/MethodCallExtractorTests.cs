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
			MethodInfo expected = TypeHelpers.GetMethod(typeof(object), ("GetHashCode"));
			Assert.Equal(expected, mi);
		}

		[Fact]
		public void ShouldExtractConstructor()
		{
			var ctor = MethodCallExtractor.ExtractConstructorCall(() => new string('c', 10));
			var expected = TypeHelpers.GetConstructors(typeof(string)).SingleOrDefault(c =>
			{
				var parms = c.GetParameters();
				return parms.Length == 2 && parms[0].ParameterType == typeof(char) && parms[1].ParameterType == typeof(int);
			});
			Assert.Equal(expected, ctor);
		}
	}
}
