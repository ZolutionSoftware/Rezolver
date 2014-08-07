using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Rezolver.Tests
{
	[TestClass]
	public class LifetimeRezolverContainerTests
	{
		[TestMethod]
		public void TestMethod1()
		{
			Mock<IRezolverContainer> parentContainerMock = new Mock<IRezolverContainer>();
			parentContainerMock.Setup(c => c.CreateLifetimeContainer()).Callback((IRezolverContainer c) => new LifetimeRezolverContainer(c));
		}
	}
}
