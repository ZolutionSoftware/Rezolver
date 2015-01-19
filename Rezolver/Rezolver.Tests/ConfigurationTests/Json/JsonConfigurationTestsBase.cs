using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration.Json;
using Rezolver.Configuration;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Generic;

namespace Rezolver.Tests.ConfigurationTests.Json
{
	/// <summary>
	/// Base test class for JSON configuration.
	/// 
	/// Contains tests that should work regardless of the adapter context factory being used.
	/// </summary>
	public class JsonConfigurationTestsBase
	{
		protected DefaultRezolver ParseConfigurationAndBuild(string json, [CallerMemberName]string testName = null)
		{
			JsonConfigurationParser parser = CreateParser();
			//gives us the chance to hack the JSON string for derived tests.
			//for the advanced tests, for example, we always remove the assemblies folder
			json = PreProcess(json);
			Console.WriteLine("Attempting to parse JSON configuration for test {0}.{1}:", GetType(), testName ?? "[Unknown]");
			Console.WriteLine(json);
			var parsed = parser.Parse(json);
			var adapter = CreateAdapter();
			var builder = adapter.CreateBuilder(parsed);
			var rezolver = CreateRezolver(builder);
			return rezolver;
		}

		protected virtual string PreProcess(string json)
		{
			return json;
		}

		protected virtual JsonConfigurationParser CreateParser()
		{
			return new JsonConfigurationParser();
		}

		protected virtual IConfigurationAdapter CreateAdapter()
		{
			return new ConfigurationAdapter();
		}

		protected virtual DefaultRezolver CreateRezolver(IRezolverBuilder builder)
		{
			return new DefaultRezolver(builder, new AssemblyRezolveTargetCompiler());
		}

		[TestMethod]
		public void ShouldRezolveInt()
		{
			string json = @"
	{
		""rezolve"": [
			{ ""System.Int32"": 1 }
		] 
	}
";
			var rezolver = ParseConfigurationAndBuild(json);
			int result = rezolver.Resolve<int>();

			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public void ShouldRezolveString()
		{
			string json = @"
{
	""rezolve"": [
		{ ""System.String"": ""Hello World!"" }
	]
}
";

			var rezolver = ParseConfigurationAndBuild(json);
			string result = rezolver.Resolve<string>();
			Assert.AreEqual("Hello World!", result);
		}

		[TestMethod]
		public void ShouldRezolveRequiresNothing_1()
		{
			//first way of doing type references (as per last two types) for the registration - 
			//use the type name as the property name, with it's value being the target (the { "$construct" : ... } bit)
			string json = @"
{
	""assemblies"": [ ""Rezolver.Tests"" ],
	""rezolve"" : [
		{ ""Rezolver.Tests.ConfigurationTests.RequiresNothing"" : { ""$construct"" : ""$self"" } }
	]
}";
			var rezolver = ParseConfigurationAndBuild(json);
			int lastInstanceNumber = RequiresNothing.LastInstanceNumber;
			var instance = rezolver.Resolve<RequiresNothing>();
			Assert.AreEqual(lastInstanceNumber + 1, instance.InstanceNumber);
		}

		
		[TestMethod]
		public void ShouldRezolveRequiresNothing_2()
		{
			//second way of doing this - an explicit 'type' member with a string value, 
			//and a value member with the target object

			//note - this is just another way of specifying a type reference in a json file by string.
			string json = @"
{
	""assemblies"": [ ""Rezolver.Tests"" ],
	""rezolve"" : [
		{
			""type"": ""Rezolver.Tests.ConfigurationTests.RequiresNothing"",
			""value"": { ""$construct"" : ""$self"" } 
		}
	]
}";

			var rezolver = ParseConfigurationAndBuild(json);
			int lastInstanceNumber = RequiresNothing.LastInstanceNumber;
			var instance = rezolver.Resolve<RequiresNothing>();
			Assert.AreEqual(lastInstanceNumber + 1, instance.InstanceNumber);
		}

		[TestMethod]
		public void ShouldRezolveRequiresNothing_3()
		{
			//third way of doing this - an explicit type member, with the value being an object that
			//describes the type reference with a base type name and potentially generic arguments.
			//this is the most verbose way to provide a type reference in a json configuration file, and
			//can be used both to describe the type against which a target will be registered, but also
			//the type to build, e.g. as the value for the $construct property of a target.
			//second way of doing this - an explicit 'type' member with a string value, 
			//and a value member with the target object
			string json = @"
{
	""assemblies"": [ ""Rezolver.Tests"" ],
	""rezolve"" : [
		{
			""type"": { ""name"": ""Rezolver.Tests.ConfigurationTests.RequiresNothing"" },
			""value"": { ""$construct"" : ""$self"" } 
		}
	]
}";

			var rezolver = ParseConfigurationAndBuild(json);
			int lastInstanceNumber = RequiresNothing.LastInstanceNumber;
			var instance = rezolver.Resolve<RequiresNothing>();
			Assert.AreEqual(lastInstanceNumber + 1, instance.InstanceNumber);
		}

		[TestMethod]
		public void ShouldRezolveRequiresInt()
		{
			//I'm not doing the same three examples as above here - that's already been tested.
			//however I am registering an entry for IRequiresInt, but building an instance of RequiresInt
			string json = @"
{
	""assemblies"":[""Rezolver.Tests"" ],
	""rezolve"": [
		{ ""System.Int32"": 105 },
		{ ""Rezolver.Tests.ConfigurationTests.IRequiresInt"" : { ""$construct"": ""Rezolver.Tests.ConfigurationTests.RequiresInt"" } }
	]
}
";
			var rezolver = ParseConfigurationAndBuild(json);
			IRequiresInt requiresInt = rezolver.Resolve<IRequiresInt>();
			Assert.AreEqual(105, requiresInt.IntValue);
		}

		[TestMethod]
		public void ShouldRezolveRequiresIntByTwoTypes()
		{
			//this time registering RequiresInt against both RequiresInt type and the interface
			//notice this time the use of 'types' instead of 'type'.  This accepts an array of type references which,
			//as mentioned in an earlier test, can be a literal string specifying a type name, or a construct such 
			//as { "name": "[typename]", "args": [ {type_reference}, ... ] }
			string json = @"{
	""assemblies"":[""Rezolver.Tests"" ],
	""rezolve"": [
		{ ""System.Int32"": 110 },
		{ 
			""types"": [ ""Rezolver.Tests.ConfigurationTests.IRequiresInt"", ""Rezolver.Tests.ConfigurationTests.RequiresInt"" ],
			""value"": { ""$construct"": ""Rezolver.Tests.ConfigurationTests.RequiresInt"" }
		}
	]
}";

			var rezolver = ParseConfigurationAndBuild(json);
			IRequiresInt requiresInt = rezolver.Resolve<IRequiresInt>();
			RequiresInt requiresInt2 = rezolver.Resolve<RequiresInt>();

			Assert.AreEqual(requiresInt.IntValue, requiresInt2.IntValue);
			Assert.AreEqual(110, requiresInt.IntValue);
			//however, they shouldn't be the same instance:
			Assert.AreNotSame(requiresInt, requiresInt2);
		}

		[TestMethod]
		public void ShouldRezolveSingletonRequiresIntByTwoTypes()
		{
			//as you can guess by the last assert in the previous test - this does the same
			//again, except this time, it's expected that we get the same instance for both rezolve calls.
			//singletons are easy - just take the value entry that you typically put, and wrap it in a { "$singleton": /* original value */ }
			string json = @"{
	""assemblies"":[""Rezolver.Tests"" ],
	""rezolve"": [
		{ ""System.Int32"": 115 },
		{ 
			""types"": [ ""Rezolver.Tests.ConfigurationTests.IRequiresInt"", ""Rezolver.Tests.ConfigurationTests.RequiresInt"" ],
			""value"": { ""$singleton"" : { ""$construct"": ""Rezolver.Tests.ConfigurationTests.RequiresInt"" } }
		}
	]
}";

			var rezolver = ParseConfigurationAndBuild(json);
			IRequiresInt requiresInt = rezolver.Resolve<IRequiresInt>();
			RequiresInt requiresInt2 = rezolver.Resolve<RequiresInt>();

			Assert.AreEqual(115, requiresInt.IntValue);
			//however, they shouldn't be the same instance:
			Assert.AreSame(requiresInt, requiresInt2);
		}

		[TestMethod]
		public void ShouldRezolveArrayOfStrings()
		{
			//here, an array of strings can be provided by a literal string array in the JSON.

			string json = @"{
	""rezolve"" : [
		{ ""System.String[]"" : [ ""Hello World0"", ""Hello World1"", ""Hello World2"" ] }
	]
}";
			var rezolver = ParseConfigurationAndBuild(json);
			string[] array = rezolver.Resolve<string[]>();
			Assert.IsNotNull(array);
			Assert.AreEqual(3, array.Length);
			Assert.IsTrue(Enumerable.Range(0, 3).Select(i => string.Format("Hello World{0}", i)).SequenceEqual(array));
		}

		[TestMethod]
		public void ShouldRezolveEnumerableOfStringsViaMultiRegistration()
		{
			//so instead of registering an array - we're registering a bunch of strings as a multiple registration
			//this then means they are retrievable as an IEnumerable<string>
			string json = @"{
	""rezolve"" : [
		{ 
			""System.String"" : {
				""$multiple"": [ ""Hello Multiple0"", ""Hello Multiple1"", ""Hello Multiple2"" ] 
			}
		}
	]
}";
			var rezolver = ParseConfigurationAndBuild(json);
			IEnumerable<string> multiple = rezolver.Resolve<IEnumerable<string>>();
			Assert.IsNotNull(multiple);
			var array = multiple.ToArray();
			Assert.AreEqual(3, array.Length); ;
			Assert.IsTrue(Enumerable.Range(0, 3).Select(i => string.Format("Hello Multiple{0}", i)).SequenceEqual(array));
		}

		[TestMethod]
		public void ShouldRezolveEnumerableOfIRequiresNothingViaMultiRegistration()
		{
			//there's a way of doing this which binds to the array/enumerable type or whatever, and then 
			//create the underlying array inline within JSON - however it's likely to require implementing
			//JSON serialization.
			//this test shows instead how we specify element type, instruct the parser that we intend to register
			//multiple targets against that, and then the caller will simply request IEnumerable<type>
			string json = @"{
	""assemblies"":[ ""Rezolver.Tests"" ],	
	""rezolve"" : [
		{
			""Rezolver.Tests.ConfigurationTests.IRequiresNothing"" :
			{ 
				""$multiple"" :
				[ 
					{ ""$construct"" : ""Rezolver.Tests.ConfigurationTests.RequiresNothing""  }, 
					{ ""$construct"" : ""Rezolver.Tests.ConfigurationTests.RequiresNothing""  }, 
					{ ""$construct"" : ""Rezolver.Tests.ConfigurationTests.RequiresNothing""  }
				]
			} 
		}
	]
}";
			var rezolver = ParseConfigurationAndBuild(json);
			IEnumerable<IRequiresNothing> multiple = rezolver.Resolve<IEnumerable<IRequiresNothing>>();
			Assert.IsNotNull(multiple);
			var array = multiple.ToArray();
			Assert.AreEqual(3, array.Length);
			Assert.AreEqual(3, array.Select(r => r.InstanceNumber).Distinct().ToArray().Length);
		}
	}
}
