using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rezolver.Examples
{
	[TestClass]
	public class SimpleDependencyExamples
	{
		public class RequiresStringIntAndDouble
		{
			public int IntegerValue { get; private set; }
			public double DoubleValue { get; private set; }
			public string StringValue { get; private set; }

			public RequiresStringIntAndDouble(
				int integerValue, 
				double doubleValue, 
				string stringValue)
			{
				IntegerValue = integerValue;
				DoubleValue = doubleValue;
				StringValue = stringValue;
			}
		}

		[TestMethod]
		public void WithSimpleDependencies()
		{
			var rezolver = new DefaultRezolver();
			rezolver.RegisterObject(10);
			rezolver.RegisterObject(25.02);
			rezolver.RegisterObject("Hello World");
			rezolver.RegisterType<RequiresStringIntAndDouble>();

			var instance = rezolver.Resolve<RequiresStringIntAndDouble>();
			Assert.AreEqual(10, instance.IntegerValue);
			Assert.AreEqual(25.02, instance.DoubleValue);
			Assert.AreEqual("Hello World", instance.StringValue);
		}
	}
}
