using Rezolver.Targets;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class TargetContainerTests
	{
		[Fact]
		public void ShouldRegisterNullObjectTarget()
		{
			// <example1>
			ITarget t = new ObjectTarget(null);
			ITargetContainer r = new TargetContainer();
			r.Register(t, serviceType: typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.Same(t, t2);
			// </example1>
		}

		[Fact]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			ITarget t = new ObjectTarget("hello world");
			ITargetContainer r = new TargetContainer();
			Assert.Throws<ArgumentException>(() => r.Register(t, serviceType: typeof(int)));
		}

		[Fact]
		public void ShouldNotAllowNullTypeOnFetch()
		{
			ITargetContainer r = new TargetContainer();
			Assert.Throws<ArgumentNullException>(() => r.Fetch(null));
		}

		[Fact]
		public void ShouldRegisterForImplicitType()
		{
			ITarget t = new ObjectTarget("hello word");
			ITargetContainer rezolverBuilder = new TargetContainer();
			rezolverBuilder.Register(t);
			var t2 = rezolverBuilder.Fetch(typeof(string));
			Assert.Same(t, t2);
		}

		[Fact]
		public void ShouldSupportTwoRegistrations()
		{
			ITargetContainer builder = new TargetContainer();
			var simpleType = new NoCtor();

            ITarget target1 = Target.ForObject("hello world");
            ITarget target2 = Target.ForObject(new NoCtor());
			builder.Register(target1);
			builder.Register(target2);
			Assert.Same(target1, builder.Fetch(typeof(string)));
			Assert.Same(target2, builder.Fetch(typeof(NoCtor)));
		}

		[Fact]
		public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
		{
			ITargetContainer builder = new TargetContainer();
			var target = Target.ForType(typeof(Generic<>));
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
			ITargetContainer builder = new TargetContainer();
			builder.RegisterType(typeof(Generic<>), typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<int>));
			Assert.NotNull(fetched);
			Assert.False(fetched.UseFallback);
		}

		[Fact]
		public void ShouldFavourSpecialisationOfGenericInt()
		{
			ITargetContainer builder = new TargetContainer();
			var notExpected = Target.ForType(typeof(Generic<>));
			var expected = Target.ForType(typeof(AltGeneric<int>));
			builder.Register(notExpected, typeof(IGeneric<>));
			builder.Register(expected, typeof(IGeneric<int>));
			var fetched = builder.Fetch(typeof(IGeneric<int>));
			Assert.NotSame(notExpected, fetched);
			Assert.Same(expected, fetched);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
		{
			ITargetContainer builder = new TargetContainer();
			var target = Target.ForType(typeof(Generic<>));
			builder.Register(target, typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
		{
			//can't think what else to call this scenario!
			ITargetContainer builder = new TargetContainer();
			var target = Target.ForType(typeof(NestedGenericA<>));
			builder.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
			var fetched = builder.Fetch(typeof(IGeneric<IEnumerable<int>>));
			Assert.Same(target, fetched);
		}

		[Fact]
		public void ShouldFavourGenericSpecialisationOfGeneric()
		{
			ITargetContainer builder = new TargetContainer();
			var target = Target.ForType(typeof(Generic<>));

			//note here - using MakeGenericType is the only way to get a reference to a type like IFoo<IFoo<>> because
			//supply an open generic as a type parameter to a generic is not valid.
			var target2 = Target.ForType(typeof(NestedGenericA<>));

			builder.Register(target, typeof(IGeneric<>));
			builder.Register(target2, typeof(IGeneric<>).MakeGenericType(typeof(IEnumerable<>)));
			var fetched = builder.Fetch(typeof(IGeneric<IEnumerable<int>>));
			Assert.Same(target2, fetched);
			var fetched2 = builder.Fetch(typeof(IGeneric<int>));
			Assert.Same(target, fetched2);
		}

		//[Fact]
		//public void ShouldSupportWideningGenericWhenConstraintsAreSpecific()
		//{
		//	//usually, if we have a target type with two parameters, then we can't use it 
		//	//when it's requested via a generic type with fewer parameters.
		//	//however, if there are type constraints involved, then we might be able to.
		//	Assert.False(true);
		//}

		public interface IMultipleRegistration
		{

		}

		public class MultipleRegistration1 : IMultipleRegistration
		{
		}

		public class MultipleRegistration2 : IMultipleRegistration
		{

		}

		[Fact]
		public void ShouldSupportRegisteringMultipleTargetsOfTheSameType()
		{
			ITargetContainer builder = new TargetContainer();
			builder.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration1>() }, typeof(IMultipleRegistration));

			var fetched = builder.Fetch(typeof(IEnumerable<IMultipleRegistration>));
			Assert.NotNull(fetched);
			Assert.False(fetched.UseFallback);
		}

		[Fact]
		public void ShouldSupportRegisteringMultipleTargetsOfDifferentTypeWithCommonInterface()
		{
			ITargetContainer targets = new TargetContainer();

			targets.RegisterMultiple(new[] { Target.ForType<MultipleRegistration1>(), Target.ForType<MultipleRegistration2>() }, typeof(IMultipleRegistration));

			var fetched = targets.Fetch(typeof(IEnumerable<IMultipleRegistration>));
			Assert.NotNull(fetched);
			Assert.False(fetched.UseFallback);
		}

		[Fact]
		public void ShouldReturnFallbackTargetForUnregisteredIEnumerable()
		{
			ITargetContainer targets = new TargetContainer();
			var result = targets.Fetch(typeof(IEnumerable<int>));
			Assert.NotNull(result);
			Assert.True(result.UseFallback);
		}

        //[Fact]
        //public void ShouldIncludeAllGenericsInEnumerable()
        //{
        //    ITargetContainer targets = new TargetContainer();

        //    var intTarget = Target.ForObject(1);
        //    var doubleTarget = Target.ForObject(1.0);

        //    targets.RegisterAll(intTarget, doubleTarget);

        //    targets.RegisterContainer(typeof(IEnumerable<object>), new EnumerableIncludeTargetContainer(typeof(object), typeof(int)));
        //}
	}
}
