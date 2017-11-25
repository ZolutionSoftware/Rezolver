using Rezolver.Tests.Types;
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
            // Arrange
			MethodInfo expected = TypeHelpers.GetMethod(typeof(object), ("GetHashCode"));

            // Act
			MethodInfo mi = Extract.Method((object o) => o.GetHashCode());

            // Assert
			Assert.Equal(expected, mi);
		}

		[Fact]
		public void ShouldExtractConstructor()
		{
            // Arrange
			var expected = TypeHelpers.GetConstructors(typeof(string)).SingleOrDefault(c =>
			{
				var parms = c.GetParameters();
				return parms.Length == 2 && parms[0].ParameterType == typeof(char) && parms[1].ParameterType == typeof(int);
			});

            // Act
			var ctor = Extract.Constructor(() => new string('c', 10));

            // Assert
			Assert.Equal(expected, ctor);
		}

        [Fact]
        public void ShouldExtractGenericConstructor()
        {
            // Arrange
            var p1Type = typeof(GenericTwoCtors<>).GetGenericArguments()[0];
            var expected = TypeHelpers.GetConstructors(typeof(GenericTwoCtors<>))
                .SingleOrDefault(c => new[] { p1Type, typeof(int) }.SequenceEqual(c.GetParameters().Select(p => p.ParameterType)));

            // Act
            var ctor = Extract.GenericConstructor(() => new Types.GenericTwoCtors<string>("", 0));

            // Assert
            Assert.Equal(expected, ctor);
        }

        [Fact]
        public void ShouldExtractGenericMethod()
        {
            // Arrange
            var expected = TypeHelpers.GetMethod(typeof(List<>), "Add");

            // Act
            var addMethod = Extract.GenericTypeMethod((List<object> l) => l.Add(null));

            // Assert
            Assert.Equal(expected, addMethod);
        }
	}
}
