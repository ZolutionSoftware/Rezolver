using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class ConstructorTargetTests : TestsBase
	{
		private class ConstructorTestClass
		{
			public int Value { get; protected set; }
		}

		private class DefaultConstructor : ConstructorTestClass
		{
			public const int ExpectedValue = -1;
			public DefaultConstructor()
			{
				Value = ExpectedValue;
			}
		}

		private class ConstructorWithDefaults : ConstructorTestClass
		{
			public const int ExpectedValue = 1;
			public ConstructorWithDefaults(int value = ExpectedValue)
			{
				Value = value;
			}
		}

		private class NoDefaultConstructor : ConstructorTestClass
		{
			public const int ExpectedRezolvedValue = 101;
			public const int ExpectedValue = 100;
			public NoDefaultConstructor(int value)
			{
				Value = value;
			}
		}

		[TestMethod]
		public void ShouldAutomaticallyFindDefaultConstructor()
		{
			var target = ConstructorTarget.For<DefaultConstructor>();
			var result = GetValueFromTarget<DefaultConstructor>(target);
			Assert.AreEqual(DefaultConstructor.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.AreNotSame(result, result2);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldThrowArgumentExceptionIfNoDefaultConstructor()
		{
			var target = ConstructorTarget.For<NoDefaultConstructor>();
		}

		[TestMethod]
		public void ShouldFindConstructorWithOptionalParameters()
		{
			//This test demonstrates whether a constructor with all-default parameters will be treated equally
			//to a default constructor if no default constructor is present on the type.
			var target = ConstructorTarget.For<ConstructorWithDefaults>();
			var result = GetValueFromTarget<ConstructorWithDefaults>(target);
			Assert.AreEqual(ConstructorWithDefaults.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.AreNotSame(result, result2);
		}

		//this test now moves into specifically selecting a constructor and extracting the parameter bindings directly
		//from the caller.  We get to automatically deriving parameter bindings for required parameters later.
		[TestMethod]
		public void ShouldAllowAllConstructorParametersToBeProvided()
		{
			var target = ConstructorTarget.For(scope => new NoDefaultConstructor(NoDefaultConstructor.ExpectedValue));
			var result = GetValueFromTarget<NoDefaultConstructor>(target);
			Assert.AreEqual(NoDefaultConstructor.ExpectedValue, result.Value);
			var result2 = GetValueFromTarget<ConstructorTestClass>(target);
			Assert.AreNotSame(result, result2);
		}

		[TestMethod]
		public void ShouldAllowAConstructorParameterToBeExplicitlyRezolved()
		{
			//this is where the action starts to heat up!
			//if we can get explicitly resolved arguments to work, then we can get easily get
			//automatically injected arguments - by simply emitting the correct expression to do the same.
			var target = ConstructorTarget.For(scope => new NoDefaultConstructor(scope.Rezolve<int>()));
			var intTarget = NoDefaultConstructor.ExpectedRezolvedValue.AsObjectTarget();
			var scopeMock = new Mock<IRezolverScope>();
			scopeMock.Setup(s => s.Fetch(typeof (int), null)).Returns(intTarget);
			var result = GetValueFromTarget<NoDefaultConstructor>(target, scopeMock.Object);
			Assert.AreEqual(NoDefaultConstructor.ExpectedRezolvedValue, result.Value);
			scopeMock.VerifyAll();
		}
	}
}
