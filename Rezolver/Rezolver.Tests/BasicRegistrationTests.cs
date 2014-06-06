using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver;

namespace Rezolver.Tests
{
	/// <summary>
	/// Tests whether the core engine will accept an object or function for a target type
	/// </summary>
	[TestClass]
	public class BasicRegistrationTests
	{
		[TestMethod]
		public void ShouldRegisterNullObjectTarget()
		{
			IRezolveTarget t = new ObjectTarget(null);
			IRezolverContainer r = new RezolverContainer();
			r.Register(t, typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.AreSame(t, t2);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			IRezolveTarget t = new ObjectTarget("hello world");
			IRezolverContainer r = new RezolverContainer();
			r.Register(t, typeof(int));
		}

		[TestMethod]
		public void ShouldRegisterForImplicitType()
		{
			IRezolveTarget t = new ObjectTarget("hello word");
			IRezolverContainer container = new RezolverContainer();
			container.Register(t);
			var t2 = container.Fetch(typeof(string));
			Assert.AreSame(t, t2);
		}
	}
}
