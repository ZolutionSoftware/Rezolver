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

		  //[TestMethod]
		  //public void StringForObject()
		  //{
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register("hello world", typeof(object));
		  //	 var o = r.Fetch(typeof(object));
		  //	 Assert.AreEqual("hello world", o);
		  //}

		  //[TestMethod]
		  //public void AValueTypeForObject()
		  //{
		  //	 int toAdd = 1;
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register(toAdd, typeof(object));
		  //	 Assert.AreEqual(toAdd, r.Fetch(typeof(object)));
		  //}

		  //[TestMethod]
		  //public void AValueTypeForNullableValueType()
		  //{
		  //	 int toAdd = 1;
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register(toAdd, typeof(int?));
		  //	 Assert.AreEqual(toAdd, r.Fetch(typeof(int?)));
		  //}

		  //[TestMethod]
		  //public void ANullableValueTypeForValueType()
		  //{
		  //	 int? toAdd = 1;
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register(toAdd, typeof(int?));
		  //	 Assert.AreEqual(toAdd, r.Fetch(typeof(int?)));
		  //}

		  //private static int IntFuncReturn = 1;
		  //private static Func<int> IntFunc = () => IntFuncReturn;

		  //[TestMethod]
		  //public void AFuncForObject_ReturningItself()
		  //{
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register(IntFunc, typeof(object));
		  //	 Assert.AreEqual(IntFunc, r.Fetch(typeof(object)));
		  //}

		  //[TestMethod]
		  //public void AFuncForObject_ReturningItsResult()
		  //{
		  //	 IRezolverContainer r = new RezolverContainer();
		  //	 r.Register(IntFunc.AsRezolverDelegate(), typeof(object));
		  //	 Assert.AreEqual(IntFuncReturn, r.Fetch(typeof(object)));
		  //}
	 }
}
