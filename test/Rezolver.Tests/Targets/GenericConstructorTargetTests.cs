using Rezolver.Targets;
using Rezolver.Tests.Types;
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

		//theory data for SupportsType and, then MapType
		public static IEnumerable<object[]> GetSupportsTypeTheoryData()
		{
			return new object[][]
			{
						//target type					//test type										//implementation type (if diff from test type)
				new[] { typeof(Generic<>),              typeof(Generic<TypeArgs.T1>)},
				new[] { typeof(Generic<>),              typeof(GenericBase<TypeArgs.T1>),				typeof(Generic<TypeArgs.T1>)},
				new[] { typeof(Generic<>),              typeof(IGeneric<TypeArgs.T1>),					typeof(Generic<TypeArgs.T1>)},
				new[] { typeof(Generic2<,>),            typeof(Generic2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(Generic2<,>),            typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>),	typeof(Generic2<TypeArgs.T1, TypeArgs.T2>) },
				new[] { typeof(ReversingGeneric2<,>),   typeof(Generic2<TypeArgs.T1, TypeArgs.T2>),		typeof(ReversingGeneric2<TypeArgs.T2, TypeArgs.T1>) },
				new[] { typeof(ReversingGeneric2<,>),   typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>),	typeof(ReversingGeneric2<TypeArgs.T2, TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(INarrowingGeneric<TypeArgs.T1>),			typeof(NarrowingGeneric<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(Generic2<TypeArgs.T1, TypeArgs.T2>),		typeof(NarrowingGeneric<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(IGeneric2<TypeArgs.T1, TypeArgs.T2>),	typeof(NarrowingGeneric<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(GenericBase<TypeArgs.T1>),				typeof(NarrowingGeneric<TypeArgs.T1>) },
				new[] { typeof(NarrowingGeneric<>),     typeof(IGeneric<TypeArgs.T1>),					typeof(NarrowingGeneric<TypeArgs.T1>) },
				//in order to map these, the algorithm has to lift the T from the IEnumerable type argument to Generic<IEnumerable<T>>
				//or IGeneric<IEnumerable<T>>
				new[] { typeof(NestedGenericA<>),		typeof(Generic<IEnumerable<TypeArgs.T1>>),		typeof(NestedGenericA<TypeArgs.T1>) },
				new[] { typeof(NestedGenericA<>),		typeof(GenericBase<IEnumerable<TypeArgs.T1>>),  typeof(NestedGenericA<TypeArgs.T1>) },
				new[] { typeof(NestedGenericA<>),		typeof(IGeneric<IEnumerable<TypeArgs.T1>>),		typeof(NestedGenericA<TypeArgs.T1>) },
				//and here, the IEnumerable<T> mapping is inherited through an interface
				new[] { typeof(NestedGenericB<>),		typeof(IGeneric<IEnumerable<TypeArgs.T1>>),		typeof(NestedGenericB<TypeArgs.T1>) },
				//double nesting of generic parameters - via a direct interface
				new[] { typeof(TwiceNestedGenericA<>),	typeof(IGeneric<IGeneric<IEnumerable<TypeArgs.T1>>>),	typeof(TwiceNestedGenericA<TypeArgs.T1>) },
				//and again, but through an interface inheriting another interface
				new[] { typeof(TwiceNestedGenericB<>),  typeof(IGeneric<IGeneric<IEnumerable<TypeArgs.T1>>>),   typeof(TwiceNestedGenericB<TypeArgs.T1>) },
				// Generic Value Type verification
				new[] { typeof(GenericValueType<>),     typeof(GenericValueType<TypeArgs.T1>) }
			};
		}

		[Theory]
		[MemberData(nameof(GetSupportsTypeTheoryData))]
		public void ShouldSupportType(Type targetType, Type testType, Type implementingType = null)
		{
			Output.WriteLine($"Testing target for type { targetType } supports { testType }...");
			var target = new GenericConstructorTarget(targetType);
			Assert.True(target.SupportsType(testType));
			//then we'll introspect the type that's going to be used to map the 
			//testType to check that it's the same as the implementingType
			var mappedType = target.MapType(testType);
			Assert.Equal(implementingType ?? testType, mappedType.Type);
		}

		public static IEnumerable<object[]> GetUnsupportedTypesTheoryData()
		{
			return new object[][] {
				//no generic constructor target should support an open generic because - well,
				//you can't create an instance of an open generic!
				new[] { typeof(Generic<>),                      typeof(Generic<>) },
				//shouldn't be supported because there's not enough type information from
				//the single type argument to GenericBase<> to bind the two type arguments
				//of Generic2
				new[] { typeof(Generic2<,>),					typeof(GenericBase<TypeArgs.T1>) }
			};
		}
	

		[Theory]
		[MemberData(nameof(GetUnsupportedTypesTheoryData))]
		public void ShouldNotSupportType(Type targetType, Type testType)
		{
			Output.WriteLine($"Testing target for type { targetType } DOES NOT support { testType }");
			var target = new GenericConstructorTarget(targetType);
			var mapping = target.MapType(testType);
			Assert.False(target.SupportsType(testType));
		}

		//bind tests merely piggy back on the supported type tests - that the target 
		//that is produced is compatible with the implementation type
		[Theory]
		[MemberData(nameof(GetSupportsTypeTheoryData))]
		public void BoundTargetShouldSupportExpectedTypes(Type targetType, Type testType, Type implementingType = null)
		{
			Output.WriteLine($"Testing target bound for type { targetType } supports { implementingType ?? testType }...");
			var target = new GenericConstructorTarget(targetType);
			var context = GetCompileContext(target, targetType: testType);
			var boundTarget = target.Bind(context);
			Assert.NotNull(boundTarget);
			Assert.True(boundTarget.SupportsType(implementingType ?? testType));
		}
	}
}
