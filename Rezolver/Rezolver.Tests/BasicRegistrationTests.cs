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
        public void ANullForObject()
        {
            IRezolverContainer r = new RezolverContainer();
            r.Register((object)null, typeof(object));
				//how do we verify that it worked?
				//we have to retrieve it!
				var o = r.Fetch(typeof(object));
				Assert.IsNull(o);
        }

		  [TestMethod]
		  public void NonNullForObject()
		  {
				object toAdd = new object();
				IRezolverContainer r = new RezolverContainer();
				r.Register(toAdd, typeof(object));
				Assert.AreEqual(toAdd, r.Fetch(typeof(object)));
		  }

		  [TestMethod]
		  public void StringForObject()
		  {
				IRezolverContainer r = new RezolverContainer();
				r.Register("hello world", typeof(object));
				var o = r.Fetch(typeof(object));
				Assert.AreEqual("hello world", o);
		  }

		  [TestMethod]
		  public void AValueTypeForObject()
		  {
				int toAdd = 1;
				IRezolverContainer r = new RezolverContainer();
				r.Register(toAdd, typeof(object));
				Assert.AreEqual(toAdd, r.Fetch(typeof(object)));
		  }

		  [TestMethod]
		  public void AValueTypeForNullableValueType()
		  {
				int toAdd = 1;
				IRezolverContainer r = new RezolverContainer();
				r.Register(toAdd, typeof(int?));
				Assert.AreEqual(toAdd, r.Fetch(typeof(int?)));
		  }

		  [TestMethod]
		  public void ANullableValueTypeForValueType()
		  {
				int? toAdd = 1;
				IRezolverContainer r = new RezolverContainer();
				r.Register(toAdd, typeof(int?));
				Assert.AreEqual(toAdd, r.Fetch(typeof(int?)));
		  }

		  private static int IntFuncReturn = 1;
		  private static Func<int> IntFunc = () => IntFuncReturn;

		  [TestMethod]
		  public void AFuncForObject_ReturningItself()
		  {
				IRezolverContainer r = new RezolverContainer();
				r.Register(IntFunc, typeof(object));
				Assert.AreEqual(IntFunc, r.Fetch(typeof(object)));
		  }

		  
    }
}
