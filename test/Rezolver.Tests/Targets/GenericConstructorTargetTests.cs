using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            // Assert
			Assert.Throws<ArgumentNullException>(() => new GenericConstructorTarget((Type)null));
		}

		[Fact]
		public void ShouldNotAllowNonGenericType()
		{
            // Assert
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(string)));
		}

		[Fact]
		public void ShouldNotAllowGenericInterfaceOrAbstractClass()
		{
            // Assert
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(IEqualityComparer<>)));
			Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(GenericBase<>)));
		}

        [Fact]
        public void ShouldNotAllowNullConstructorInfo()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new GenericConstructorTarget((ConstructorInfo)null));
        }

        [Fact]
        public void ShouldNotAllowNonGenericTypeConstructor()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(Extract.Constructor(() => new Types.BaseClass())));
        }

        [Fact]
        public void ShouldNotAllowGenericAbstractClassConstructor()
        {
            // Assert
            Assert.Throws<ArgumentException>(() => new GenericConstructorTarget(typeof(AbstractGeneric<>).GetConstructor(Type.EmptyTypes)));
        }

        [Fact]
		public void DeclaredTypeShouldBeEqualToGenericType()
		{
            // Assert
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
				new[] { typeof(GenericValueType<>),     typeof(GenericValueType<TypeArgs.T1>) },
				// TODO: using constraints expressed in terms of correctly mapped type parameters to be clever
				//new[] { typeof(ValidWideningGeneric<,>),typeof(IGeneric<int>),                          typeof(ValidWideningGeneric<IEnumerable<int>, int>) },
				// Not-fully-bound tests.  These types yield SupportsType = true, but Bind will throw an exception
				new[] { typeof(Generic<>),              typeof(Generic<>) },
				new[] { typeof(Generic<>),              typeof(IGeneric<>),								typeof(Generic<>) },
			};
		}

		[Theory]
		[MemberData(nameof(GetSupportsTypeTheoryData))]
		public void ShouldSupportTypeIfMappingIsSuccessful(Type targetType, Type testType, Type implementingType = null)
		{
			Output.WriteLine($"Testing target for type { targetType } supports { testType }...");
            // Arrange
			var target = new GenericConstructorTarget(targetType);
            
            // Act
			var mapping = target.MapType(testType);

            // Assert
			Assert.True(mapping.Success);
			Assert.Equal(implementingType ?? testType, mapping.Type);
			Assert.True(target.SupportsType(testType));
		}
		
		[Theory]
		[MemberData(nameof(GetSupportsTypeTheoryData))]
		public void ShouldBindIfFullyBoundOrErrorIfNot(Type targetType, Type testType, Type implementingType = null)
		{
			// similar test to above, but this checks the interaction between SupportsType and Bind -
			// in that some types are supported, but, when bound, will yield an exception because the mapping
			// cannot be fully bound.
			Output.WriteLine($"Testing target bound for type { targetType } supports { implementingType ?? testType }...");

            // Arrange
			var target = new GenericConstructorTarget(targetType);
			var mapping = target.MapType(testType);

            // Assert (with some 'Act' thrown in too :$)
			Assert.True(mapping.Success);

			var context = GetCompileContext(target, targetType: testType);
			if (mapping.IsFullyBound)
			{
				var boundTarget = target.Bind(context);
				Assert.NotNull(boundTarget);
			}
			else
			{
				//not specifying the type of exception we want - only that it should throw one.
				Assert.ThrowsAny<Exception>(() => target.Bind(context));
			}
		}

		public static IEnumerable<object[]> GetUnsupportedTypesTheoryData()
		{
			return new object[][] {
				//shouldn't be supported because there's not enough type information from
				//the single type argument to GenericBase<> to bind the two type arguments
				//of Generic2
				new[] { typeof(Generic2<,>),					typeof(GenericBase<TypeArgs.T1>) },
				//not supported because the types aren't even compatible
				new[] { typeof(Generic<>),						typeof(IEnumerable<TypeArgs.T1>) },
				//not supported becuse NestedGenericA<> inherits Generic<IEnumerable<>>, not Generic<>
				new[] { typeof(NestedGenericA<>),				typeof(Generic<>) }		
			};
		}
	

		[Theory]
		[MemberData(nameof(GetUnsupportedTypesTheoryData))]
		public void ShouldNotSupportType(Type targetType, Type testType)
		{
			Output.WriteLine($"Testing target for type { targetType } DOES NOT support { testType }");

            // Arrange
			var target = new GenericConstructorTarget(targetType);
			//also make sure we get an exception if we try to bind directly
			var context = GetCompileContext(target, targetType: testType);

            // Assert
			Assert.False(target.SupportsType(testType));
			Assert.ThrowsAny<Exception>(() => target.Bind(context));
		}

		[Fact]
        public void ShouldNotSupportGenericWhichViolatesConstraints()
        {
            // Arrange
            var target = new GenericConstructorTarget(typeof(ConstrainedGeneric<>));

            // Assert
            Assert.False(target.SupportsType(typeof(IGeneric<string>)));
        }

        [Fact]
        public void ShouldAcceptSuppliedConstructor()
        {
            // Arrange

            //var target = new GenericConstructorTarget
        }

        private interface ISingleGeneric<T> { }
        private interface IDoubleGeneric<T1, T2> { }
        private class DoubleGeneric<T1, T2> : ISingleGeneric<IDoubleGeneric<T1, T2>> { }

        [Fact]
        public void ShouldMapMultipleParametersThroughNonGenericBase()
        {
            var target = new GenericConstructorTarget(typeof(DoubleGeneric<,>));

            var result = target.MapType(typeof(ISingleGeneric<IDoubleGeneric<int, float>>));

            Assert.True(result.Success);
            Assert.True(result.IsFullyBound);

            Assert.Equal(typeof(DoubleGeneric<int, float>), result.Type);
        }
	}
}
