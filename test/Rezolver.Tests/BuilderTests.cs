using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class BuilderTests
	{
		[Fact]
		public void ShouldRegisterNullObjectTarget()
		{
			ITarget t = new ObjectTarget(null);
			ITargetContainer r = new Builder();
			r.Register(t, serviceType: typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.Same(t, t2);
		}

		[Fact]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			ITarget t = new ObjectTarget("hello world");
			ITargetContainer r = new Builder();
			Assert.Throws<ArgumentException>(() => r.Register(t, serviceType: typeof(int)));
		}

		[Fact]
		public void ShouldNotAllowNullTypeOnFetch()
		{
			ITargetContainer r = new Builder();
			Assert.Throws<ArgumentNullException>(() => r.Fetch(null));
		}

		[Fact]
		public void ShouldRegisterForImplicitType()
		{
			ITarget t = new ObjectTarget("hello word");
			ITargetContainer rezolverBuilder = new Builder();
			rezolverBuilder.Register(t);
			var t2 = rezolverBuilder.Fetch(typeof(string));
			Assert.Same(t, t2);
		}

		[Fact]
		public void ShouldSupportTwoRegistrations()
		{
			ITargetContainer builder = new Builder();
			var simpleType = new SimpleType();

			ITarget target1 = "hello world".AsObjectTarget();
			ITarget target2 = new SimpleType().AsObjectTarget();
			builder.Register(target1);
			builder.Register(target2);
			Assert.Same(target1, builder.Fetch(typeof(string)));
			Assert.Same(target2, builder.Fetch(typeof(SimpleType)));
		}

		[Fact]
		public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
		{
			ITargetContainer builder = new Builder();
			var target = GenericConstructorTarget.Auto(typeof(Generic<>));
      builder.Register(target, typeof(IGeneric<>));
			///this should be trivial
			var fetched = builder.Fetch(typeof(IGeneric<>));
			Assert.Same(target, fetched);
			var fetchedClosed = builder.Fetch(typeof(IGeneric<int>));
			Assert.Same(target, fetchedClosed);
		}

		[Fact]
		public void ShouldSupportRegisteringSpecialisationOfGeneric()
		{
			ITargetContainer builder = new Builder();
			builder.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<int>));
			Assert.NotNull(fetched);
			Assert.False(fetched.UseFallback);
		}

		[Fact]
		public void ShouldFavourSpecialisationOfGenericInt()
		{
			ITargetContainer builder = new Builder();
			var notExpected = ConstructorTarget.Auto(typeof(Generic<>));
			var expected = ConstructorTarget.Auto(typeof(GenericNoCtor<int>));
			builder.Register(notExpected, typeof(IGeneric<>));
			builder.Register(expected, typeof(IGeneric<int>));
			var fetched = builder.Fetch(typeof(IGeneric<int>));
			Assert.NotSame(notExpected, fetched);
			Assert.Same(expected, fetched);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
		{
			ITargetContainer builder = new Builder();
			var target = GenericConstructorTarget.Auto(typeof(Generic<>));
			builder.Register(target, typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
		{
			//can't think what else to call this scenario!
			ITargetContainer builder = new Builder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGeneric<>));
			builder.Register(target, typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched);
		}

		[Fact]
		public void ShouldFavourGenericSpecialisationOfGeneric()
		{
			ITargetContainer builder = new Builder();
			var target = GenericConstructorTarget.Auto(typeof(Generic<>));

			//note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
			//supply an open generic as a type parameter to a generic is not valid.
			var target2 = GenericConstructorTarget.Auto(typeof(GenericGeneric<>));

			builder.Register(target, typeof(IGeneric<>));
			builder.Register(target2, typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target2, fetched);
			var fetched2 = builder.Fetch(typeof(IGeneric<int>));
			Assert.Same(target, fetched2);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase2()
		{
			//subtle different between how this one is registered versus the previous test
			// - registers for IGeneric<IGeneric<T>> instead of IGeneric<T>
			ITargetContainer builder = new Builder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGeneric<>));
			builder.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched);
		}



		public interface IMultipleRegistration
		{

		}

		public class MultipleRegistration1 : IMultipleRegistration
		{
		}

		[Fact]
		public void ShouldSupportRegisteringMultipleImplementationsOfTheSameType()
		{
			ITargetContainer builder = new Builder();
			builder.RegisterMultiple(new[] { ConstructorTarget.Auto<MultipleRegistration1>(), ConstructorTarget.Auto<MultipleRegistration1>() }, typeof(IMultipleRegistration));

			var fetched = builder.Fetch(typeof(IEnumerable<IMultipleRegistration>));
			Assert.NotNull(fetched);
		}


	}
}
