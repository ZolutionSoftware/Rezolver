using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class DynamicRezolverTests : TestsBase
	{
		[TestMethod]
		public void ShouldRezolveFromDynamicContainer()
		{
			//this is using constructorTarget with a prescribed new expression
			var builderMock = new Mock<IRezolverBuilder>();
			//scope1Mock.Setup(s => s.Fetch(typeof(int), null)).Returns(new RezolvedTarget(typeof(int)));

			//the thing being that the underlying Builder does not know how too resolve an integer without
			//being passed a dynamic container at call-time.

			var rezolverMock = new Mock<IRezolver>();
			rezolverMock.Setup(c => c.CanResolve(typeof(int), null, null)).Returns(true);
			int expected = -1;
			rezolverMock.Setup(c => c.Resolve(typeof(int), null, null)).Returns(expected);
			Rezolver rezolver = new Rezolver(builderMock.Object);

			Assert.AreEqual(expected, rezolver.Resolve(typeof(int), dynamicRezolver: rezolverMock.Object));
		}
	}
}
