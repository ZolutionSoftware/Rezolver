using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Examples
{
	[TestClass]
	public class ComplexDependencyExamples
	{
		public class NoDependency
		{

		}

		public class HasDependency
		{
			public NoDependency NoDependency { get; private set; }
			public int IntegerValue { get; private set; }
			public HasDependency(
				NoDependency noDependency,
				int integerValue)
			{
				Assert.IsNotNull(noDependency);
				Assert.AreEqual(5, integerValue);
				NoDependency = noDependency;
				IntegerValue = integerValue;
			}
		}

		public class HasComplexDependency
		{
			public NoDependency NoDependency { get; private set; }
			public HasDependency HasDependency { get; private set; }
			public string StringValue { get; private set; }
			public HasComplexDependency(
				NoDependency noDependency,  
				HasDependency hasDependency,
				string stringValue)
			{
				Assert.IsNotNull(noDependency);
				Assert.IsNotNull(hasDependency);
				Assert.AreEqual("Hello Complex", stringValue);

				NoDependency = noDependency;
				HasDependency = hasDependency;
				StringValue = stringValue;
			}
		}

		[TestMethod]
		public void WithComplexDependencies()
		{
			//please note - you don't need to register dependencies
			//in least-dependant-first order.  For example, here, we
			//are registering our most dependant type first.
			//However, you won't be able to resolve an instance until
			//you've 
			var container = new DefaultRezolver();
			container.RegisterType<HasComplexDependency>();
			container.RegisterObject(5);
			container.RegisterObject("Hello Complex");
			container.RegisterType<NoDependency>();
			container.RegisterType<HasDependency>();

			var result = container.Resolve<HasComplexDependency>();
			Assert.IsNotNull(result);
		}
	}
}
