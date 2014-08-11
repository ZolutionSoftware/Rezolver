using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Tests.Classes;

namespace Rezolver.Tests
{
	/// <summary>
	/// Tests whether the core engine will accept an object or function for a target type
	/// </summary>
	[TestClass]
	public class BasicBuilderTests
	{
		[TestMethod]
		public void ShouldRegisterNullObjectTarget()
		{
			IRezolveTarget t = new ObjectTarget(null);
			IRezolverBuilder r = new RezolverBuilder();
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
			IRezolverBuilder container = new RezolverBuilder();
			container.Register(t);
			var t2 = container.Fetch(typeof(string));
			Assert.AreSame(t, t2);
		}

		[TestMethod]
		public void ShouldSupportTwoRegistrations()
		{
			IRezolverBuilder container = new RezolverBuilder();
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
