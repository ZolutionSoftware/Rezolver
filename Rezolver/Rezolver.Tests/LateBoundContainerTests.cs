using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class LateBoundContainerTests : TestsBase
	{
		[TestMethod]
		public void ShouldRezolveFromDynamicContainer()
		{
			//this is using constructorTarget with a prescribed new expression
			var scope1Mock = new Mock<IRezolverBuilder>();
			//scope1Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(new RezolvedTarget(typeof(int)));

			//the thing being that the underlying Builder does not know how too resolve an integer without
			//being passed a dynamic container at call-time.

			var containerMock = new Mock<IRezolver>();
			containerMock.Setup(c => c.CanResolve(typeof(int), null, null)).Returns(true);
			int expected = -1;
			containerMock.Setup(c => c.Resolve(typeof(int), null, null)).Returns(expected);
			Rezolver container = new Rezolver(scope1Mock.Object);

			Assert.AreEqual(expected, container.Resolve(typeof(int), dynamic: containerMock.Object));
		}
	}
}
