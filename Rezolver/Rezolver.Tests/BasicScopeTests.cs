using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Tests.Classes;

namespace Rezolver.Tests
{
	/// <summary>
	/// Tests whether the core engine will accept an object or function for a target type
	/// </summary>
	[TestClass]
	public class BasicScopeTests
	{
		[TestMethod]
		public void ShouldRegisterNullObjectTarget()
		{
			IRezolveTarget t = new ObjectTarget(null);
			IRezolverScope r = new RezolverScope();
			r.Register(t, type: typeof(object));
			var t2 = r.Fetch(typeof(object));
			Assert.AreSame(t, t2);

			var t3 = r.Fetch<object>();
			Assert.AreSame(t, t3);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void ShouldNotRegisterIfTypesDontMatch()
		{
			IRezolveTarget t = new ObjectTarget("hello world");
			IRezolverScope r = new RezolverScope();
			r.Register(t, type: typeof(int));
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldNotAllowNullTypeOnFetch()
		{
			IRezolverScope r = new RezolverScope();
			r.Fetch(null);
		}

		[TestMethod]
		public void ShouldRegisterForImplicitType()
		{
			IRezolveTarget t = new ObjectTarget("hello word");
			IRezolverScope container = new RezolverScope();
			container.Register(t);
			var t2 = container.Fetch(typeof(string));
			Assert.AreSame(t, t2);
		}

		[TestMethod]
		public void ShouldSupportTwoRegistrations()
		{
			IRezolverScope container = new RezolverScope();
			var simpleType = new SimpleType();

			IRezolveTarget target1 = "hello world".AsObjectTarget();
			IRezolveTarget target2 = new SimpleType().AsObjectTarget();
			container.Register(target1);
			container.Register(target2);
			Assert.AreEqual(target1, container.Fetch(typeof(string)));
			Assert.AreEqual(target2, container.Fetch(typeof (SimpleType)));
		}


	}
}
