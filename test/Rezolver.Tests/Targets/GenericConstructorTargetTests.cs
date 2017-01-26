using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Rezolver.Tests.Targets
{
	public class GenericConstructorTargetTests : TargetTestsBase
	{
		public GenericConstructorTargetTests(ITestOutputHelper output)
			: base(output)
		{

		}

		[Fact]
		public void ShouldNotAllowNullType()
		{
			Assert.Throws<ArgumentNullException>(() => new GenericConstructorTarget(null));
		}

		[Fact]
		public void ShouldNotAllowNonGenericType()
		{
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(string)));
		}

		[Fact]
		public void ShouldNotAllowGenericInterfaceOrAbstractClass()
		{
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(IEqualityComparer<>)));
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(GenericBase<>)));
		}

		[Fact]
		public void DeclaredTypeShouldBeEqualToGenericType()
		{
			Assert.Same(typeof(Generic<>), new GenericConstructorTarget(typeof(Generic<>)).DeclaredType);
		}

		//since the class overrides SupportsType - we use a theory to test
		public static IEnumerable<object[]> GetSupportsTypeTheoryData()
		{
			return new object[][]
			{
						//target type					//type which should be supported (often alternating between ref/value types)
				new[] { typeof(Generic<>),              typeof(Generic<TypeArgs.T1>)},
				new[] { typeof(Generic<>),              typeof(GenericBase<TypeArgs.T1>)},
				new[] { typeof(Generic<>),              typeof(IGeneric<TypeArgs.T1>)},
				new[] { typeof(Generic2<,>),            typeof(Generic2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(Generic2<,>),            typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(ReversingGeneric2<,>),   typeof(Generic2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(ReversingGeneric2<,>),   typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(INarrowingGeneric<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(Generic2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(GenericBase<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(IGeneric<TypeArgs.T1>) },
			};
		}

		[Theory]
		[MemberData(nameof(GetSupportsTypeTheoryData))]
		public void ShouldSupportType(Type targetType, Type testType)
		{
			Output.WriteLine($"Testing target for type { targetType } supports { testType }...");
			var target = new GenericConstructorTarget(targetType);
			Assert.True(target.SupportsType(testType));
		}

		public static IEnumerable<object[]> GetUnsupportedTypesTheoryData()
		{
			return new object[][] {
				new[] { typeof(Generic2<,>), typeof(GenericBase<TypeArgs.T1>) }
			};
#error add other types - like Open Generics etc.
		}
	

		[Theory]
		[MemberData(nameof(GetUnsupportedTypesTheoryData))]
		public void ShouldNotSupportType(Type targetType, Type testType)
		{
			Output.WriteLine($"Testing target for type { targetType } DOES NOT support { testType }");
			var target = new GenericConstructorTarget(targetType);
			Assert.False(target.SupportsType(testType));
		}
	}
}
