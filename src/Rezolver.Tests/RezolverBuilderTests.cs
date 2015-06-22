﻿using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rezolver.Tests.Classes;

namespace Rezolver.Tests
{
	/// <summary>
	/// Tests whether the core engine will accept an object or function for a target type
	/// </summary>
	[TestClass]
	public class RezolverBuilderTests
	{
		[TestMethod]
		public void ShouldRegisterNullObjectTarget()
		{
			IRezolveTarget t = new ObjectTarget(null);
			IRezolverBuilder r = new RezolverBuilder();
			r.Register(t, type: typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.AreSame(t, t2.DefaultTarget);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			IRezolveTarget t = new ObjectTarget("hello world");
			IRezolverBuilder r = new RezolverBuilder();
			r.Register(t, type: typeof(int));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullTypeOnFetch()
		{
			IRezolverBuilder r = new RezolverBuilder();
			r.Fetch(null);
		}

		[TestMethod]
		public void ShouldRegisterForImplicitType()
		{
			IRezolveTarget t = new ObjectTarget("hello word");
			IRezolverBuilder rezolverBuilder = new RezolverBuilder();
			rezolverBuilder.Register(t);
			var t2 = rezolverBuilder.Fetch(typeof(string));
			Assert.AreSame(t, t2.DefaultTarget);
		}

		[TestMethod]
		public void ShouldSupportTwoRegistrations()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var simpleType = new SimpleType();

			IRezolveTarget target1 = "hello world".AsObjectTarget();
			IRezolveTarget target2 = new SimpleType().AsObjectTarget();
			builder.Register(target1);
			builder.Register(target2);
			Assert.AreSame(target1, builder.Fetch(typeof(string)).DefaultTarget);
			Assert.AreSame(target2, builder.Fetch(typeof (SimpleType)).DefaultTarget);
		}

		private interface IGenericTest<T>
		{

		}

		private class GenericTest<T> : IGenericTest<T>
		{

		}

		public class GenericGenericTest<T> : IGenericTest<IGenericTest<T>>
		{
			//complex scenario - implementing an interface that wraps another generic interface with the supplied type parameter
		}

		[TestMethod]
		public void ShouldSupportRegisteringOpenGenericAndFetchingAsClosed()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var targetMock = new Mock<IRezolveTarget>();
			targetMock.Setup(m => m.DeclaredType).Returns(typeof(IGenericTest<>));
			targetMock.Setup(m => m.SupportsType(It.IsAny<Type>())).Returns((Type t) => {
				if(t == typeof(IGenericTest<>))
					return true;
				if(t.IsGenericType)
				{
					if(t.GetGenericTypeDefinition() == typeof(IGenericTest<>))
						return true;
				}

				return t == typeof(object);
			});
			var mockInstance = targetMock.Object;
			builder.Register(mockInstance);
			///this should be trivial
			var fetched = builder.Fetch(typeof(IGenericTest<>));
			Assert.AreSame(mockInstance, fetched.DefaultTarget);
			var fetchedClosed = builder.Fetch(typeof(IGenericTest<int>));
			Assert.AreSame(mockInstance, fetchedClosed.DefaultTarget);
		}

		[TestMethod]
		public void ShouldSupportRegisteringAndRetrievingGenericWithGenericParameter()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(GenericTest<>));
			builder.Register(target, typeof(IGenericTest<>));
			var fetched = builder.Fetch(typeof(IGenericTest<IGenericTest<int>>));
			Assert.AreSame(target, fetched.DefaultTarget);
		}

		[TestMethod]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase()
		{
			//can't think what else to call this scenario!
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGenericTest<>));
			builder.Register(target, typeof(IGenericTest<>));
			var fetched = builder.Fetch(typeof(IGenericTest<IGenericTest<int>>));
			Assert.AreSame(target, fetched.DefaultTarget);
		}

		[TestMethod]
		public void ShouldSupportRegisteringAndRetrievingGenericWithAsymmetricGenericBase2()
		{
			//subtle different between how this one is registered versus the previous test
			IRezolverBuilder builder = new RezolverBuilder();
			var target = GenericConstructorTarget.Auto(typeof(GenericGenericTest<>));
			builder.Register(target, typeof(IGenericTest<>).MakeGenericType(typeof(IGenericTest<>)));
			var fetched = builder.Fetch(typeof(IGenericTest<IGenericTest<int>>));
			Assert.AreSame(target, fetched.DefaultTarget);
		}

		public interface IMultipleRegistration
		{

		}

		public class MultipleRegistration1 : IMultipleRegistration 
		{
		}

		public class MultipleRegistration2 : IMultipleRegistration
		{
		}

		[TestMethod]
		public void ShouldSupportRegisteringMultipleImplementationsOfTheSameType()
		{
			IRezolverBuilder builder = new RezolverBuilder();
			builder.RegisterMultiple(new[] { ConstructorTarget.Auto<MultipleRegistration1>(), ConstructorTarget.Auto<MultipleRegistration1>()}, typeof(IMultipleRegistration));
			//builder.RegisterMultiple(ConstructorTarget.Auto<MultipleRegistration2>(), targetType: typeof(IMultipleRegistration));

			var fetched = builder.Fetch(typeof(IEnumerable<IMultipleRegistration>));
			Assert.IsNotNull(fetched);
		}
	}
}