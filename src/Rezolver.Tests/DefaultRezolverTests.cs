using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class DefaultRezolverTests : TestsBase
	{
		[TestMethod]
		public void ShouldRezolveAnInt()
		{
			var builderMock = new Mock<IRezolverBuilder>();
			builderMock.Setup(s => s.Fetch(typeof(int), null)).Returns(CreateRezolverEntryForTarget(1.AsObjectTarget(), typeof(int)));

			IRezolver rezolver = new DefaultRezolver(builderMock.Object, new RezolveTargetDelegateCompiler());
			var result = rezolver.Resolve(typeof (int));
			Assert.AreEqual(1, result);
		}

		private class TypeWithConstructorArg
		{
			public int Value { get; private set; }

			public TypeWithConstructorArg(int value)
			{
				Value = value;
			}
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ShouldFailBecauseOfRecursiveRezolve()
		{
			//this test was originally written as a test for handling dynamically provided
			//containers - however it surfaced an exceptional condition whereby a RezolveTarget
			//could end up rezolving itself during expression compilation, leading to a stack overflow.
			//So the test evolved into one which tests that circular references, whereby a rezolve
			//target can end up rezolving back to itself, were identified and handled.
			//This check is built into RezolveTargetBase and required the addition of a stack parameter
			//to the CreateExpression method on the IRezolveTarget interface.


			//this is using constructorTarget with a prescribed new expression
			var builder1Mock = new Mock<IRezolverBuilder>();
			builder1Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(CreateRezolverEntryForTarget(new RezolvedTarget(typeof(int)), typeof(int)));

			var rezolverMock = new Mock<IRezolver>();
			rezolverMock.Setup(c => c.CanResolve(It.Is((RezolveContext r) => r.RequestedType == typeof(int)))).Returns(true);
			int expected = -1;
			rezolverMock.Setup(c => c.Resolve(It.Is((RezolveContext r) => r.RequestedType == typeof(int)))).Returns(expected);
			DefaultRezolver rezolver = new DefaultRezolver(builder1Mock.Object, new RezolveTargetDelegateCompiler());
			var result = rezolver.Resolve(typeof (int) /*, @dynamic: containerMock.Object*/);
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void ShouldRezolveIntFromDynamicallyDefinedScope()
		{

			//this is using constructorTarget with a prescribed new expression
			var builder1Mock = new Mock<IRezolverBuilder>();
			builder1Mock.Setup(s => s.Fetch(typeof(TypeWithConstructorArg), null)).Returns(CreateRezolverEntryForTarget(ConstructorTarget.Auto<TypeWithConstructorArg>(), typeof(TypeWithConstructorArg)));

			//the thing being that the underlying Builder does not know how too resolve an integer without
			//being passed a dynamic container at call-time.
			//this mocks a dynamically defined container that an application creates in response to transient information only
			var builder2Mock = new Mock<IRezolverBuilder>();
			int expected = -1;
			builder2Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(CreateRezolverEntryForTarget(expected.AsObjectTarget(), typeof(int)));
			//this represents building an application's statically defined, or bootstrapped, IOC container
			DefaultRezolver rezolver = new DefaultRezolver(builder1Mock.Object, new RezolveTargetDelegateCompiler());
			CombinedRezolver dynamicRezolver = new CombinedRezolver(rezolver, builder2Mock.Object, rezolver.Compiler);

			var result = (TypeWithConstructorArg)dynamicRezolver.Resolve(typeof (TypeWithConstructorArg));
			Assert.IsNotNull(result);

			Assert.AreEqual(expected, result.Value);
		}


	}
}
