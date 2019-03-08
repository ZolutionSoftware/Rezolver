using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
    public class DefaultTargetTests : TargetTestsBase
    {
		public DefaultTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullTypeOnConstruction()
		{
			Assert.Throws<ArgumentNullException>("type", () => new DefaultTarget(null));
		}

		public static IEnumerable<object[]> TheoryTypes()
		{
			return new object[][]
			{
				new[] { typeof(int) },
				new[] { typeof(int?) },
				new[] { typeof(string) }
			};
		}

		[Theory]
		[MemberData(nameof(TheoryTypes))]
		public void DeclaredTypeShouldEqualConstructedType(Type expectedType)
		{
			Output.WriteLine($"Expected type is { expectedType }");
			Assert.Equal(expectedType, new DefaultTarget(expectedType).DeclaredType);
		}

		private static class DefaultHelper<T>
		{
			public static readonly T Value = default(T);
		}

		private static object GetDefault(Type expectedType)
		{
			return typeof(DefaultHelper<>)
							.MakeGenericType(expectedType)
							.GetStaticFields()
							.Single(f => f.Name == nameof(DefaultHelper<object>.Value))
							.GetValue(null);
		}

		[Theory]
		[MemberData(nameof(TheoryTypes))]
		public void ValueShouldReturnCorrectDefault(Type expectedType)
		{
			Output.WriteLine($"Expected type is { expectedType }");
			object expected = GetDefault(expectedType);
			Assert.Equal(expected, new DefaultTarget(expectedType).Value);
		}

		[Theory]
		[MemberData(nameof(TheoryTypes))]
		public void CompiledTargetImplementationShouldReturnCorrectDefault(Type expectedType)
		{
			Output.WriteLine($"Expected type is { expectedType }");
			object expected = GetDefault(expectedType);
			ICompiledTarget toTest = new DefaultTarget(expectedType);
			//shouldn't need a ResolveContext
			Assert.Equal(expected, toTest.GetObject(default));
		}
	}
}
