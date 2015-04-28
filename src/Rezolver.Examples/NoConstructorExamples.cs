using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Examples
{
	//these tests are just used as containers for the code examples on the website

	[TestClass]
	public class NoConstructorExamples
	{
		public class NoConstructor
		{
			private static int _counter;
			public static int Counter { get { return _counter; } }

			public int InstanceID { get; private set; }

			public NoConstructor()
			{
				InstanceID = ++_counter;
			}
		}
		[TestMethod]
		public void CreateContainerAndRegisterType()
		{
			IRezolver rezolver = new DefaultRezolver();

			//register a type - will find the greediest constructor to bind.
			rezolver.RegisterType<NoConstructor>();
			
			// can also use a non-generic version
			//rezolver.RegisterType(typeof(NoConstructor));

			int expected = NoConstructor.Counter + 1;

			//the Rezolve operation is a member of IRezolver
			//that has a bunch of extensions
			var instance = rezolver.Resolve<NoConstructor>();

			Assert.AreEqual(expected, instance.InstanceID);
			//grab another instance to show that the counter is increased
			Assert.AreEqual(expected + 1, 
				rezolver.Resolve<NoConstructor>().InstanceID);
		}
	}
}
