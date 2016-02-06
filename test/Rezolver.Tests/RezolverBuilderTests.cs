using Rezolver.Tests.TestTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests
{
	public class RezolverBuilderTests
	{
		[Fact]
		public void ShouldRegisterNullObjectTarget()
		{
			IRezolveTarget t = new ObjectTarget(null);
			IRezolverBuilder r = new RezolverBuilder();
			r.Register(t, serviceType: typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.Same(t, t2.DefaultTarget);
		}

		[Fact]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			IRezolveTarget t = new ObjectTarget("hello world");
			IRezolverBuilder r = new RezolverBuilder();
			Assert.Throws<ArgumentException>(() => r.Register(t, serviceType: typeof(int)));
		}

		[Fact]
		public void ShouldNotAllowNullTypeOnFetch()
		{
			IRezolverBuilder r = new RezolverBuilder();
			Assert.Throws<ArgumentNullException>(() => r.Fetch(null));
		}

		[Fact]
		public void ShouldRegisterForImplicitType()
		{
			IRezolveTarget t = new ObjectTarget("hello word");
			IRezolverBuilder rezolverBuilder = new RezolverBuilder();
			rezolverBuilder.Register(t);
			var t2 = rezolverBuilder.Fetch(typeof(string));
			Assert.Same(t, t2.DefaultTarget);
		}

		[Fact]
		public void ShouldSupportTwoRegistrations()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var simpleType = new SimpleType();

			IRezolveTarget target1 = "hello world".AsObjectTarget();
			IRezolveTarget target2 = new SimpleType().AsObjectTarget();
			builder.Register(target1);
			builder.Register(target2);
			Assert.Same(target1, builder.Fetch(typeof(string)).DefaultTarget);
			Assert.Same(target2, builder.Fetch(typeof(SimpleType)).DefaultTarget);
		}

		[Fact]
		public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(Generic<>));
      builder.Register(target, typeof(IGeneric<>));
			///this should be trivial
			var fetched = builder.Fetch(typeof(IGeneric<>));
			Assert.Same(target, fetched.DefaultTarget);
			var fetchedClosed = builder.Fetch(typeof(IGeneric<int>));
			Assert.Same(target, fetchedClosed.DefaultTarget);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(Generic<>));
			builder.Register(target, typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched.DefaultTarget);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
		{
			//can't think what else to call this scenario!
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGeneric<>));
			builder.Register(target, typeof(IGeneric<>));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched.DefaultTarget);
		}

		[Fact]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase2()
		{
			//subtle different between how this one is registered versus the previous test
			// - registers for IGeneric<IGeneric<T>> instead of IGeneric<T>
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGeneric<>));
			builder.Register(target, typeof(IGeneric<>).MakeGenericType(typeof(IGeneric<>)));
			var fetched = builder.Fetch(typeof(IGeneric<IGeneric<int>>));
			Assert.Same(target, fetched.DefaultTarget);
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
			IRezolverBuilder builder = new RezolverBuilder();
			builder.RegisterMultiple(new[] { ConstructorTarget.Auto<MultipleRegistration1>(), ConstructorTarget.Auto<MultipleRegistration1>() }, typeof(IMultipleRegistration));

			var fetched = builder.Fetch(typeof(IEnumerable<IMultipleRegistration>));
			Assert.NotNull(fetched);
			Assert.Equal(2, fetched.Targets.Count());
		}
	}
}
