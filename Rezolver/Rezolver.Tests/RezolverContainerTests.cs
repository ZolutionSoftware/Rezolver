using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class RezolverContainerTests
	{
		[TestMethod]
		public void ShouldRezolveAnInt()
		{
			var scopeMock = new Mock<IRezolverScope>();
			scopeMock.Setup(s => s.Fetch(typeof(int), null)).Returns(1.AsObjectTarget());

			IRezolverContainer container = new RezolverContainer(scopeMock.Object);
			var result = container.Rezolve(typeof (int));
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
		public void ShouldRezolveFromDynamicContainer()
		{
			//this is using constructorTarget with a prescribed new expression
			var scope1Mock = new Mock<IRezolverScope>();
			scope1Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(new RezolvedTarget(typeof(int)));

			//the thing being that the underlying scope does not know how too resolve an integer without
			//being passed a dynamic container at call-time.

			var containerMock = new Mock<IRezolverContainer>();
			containerMock.Setup(c => c.CanResolve(typeof(int), null, null)).Returns(true);
			int expected = -1;
			containerMock.Setup(c => c.Rezolve(typeof(int), null, null)).Returns(expected);
			RezolverContainer container = new RezolverContainer(scope1Mock.Object);

			Assert.AreEqual(expected, container.Rezolve(typeof(int), dynamicContainer: containerMock.Object));
		}
	}
}
