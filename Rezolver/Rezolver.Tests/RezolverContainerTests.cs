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
	}
}
