using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration.Json;
using Rezolver.Configuration;
using System.Runtime.CompilerServices;

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
			//fiirst way of doing type references (as per last two types) for the registration - 
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
	}
}
