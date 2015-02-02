using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration;
using Rezolver.Configuration.Json;
using System.Collections.Generic;

namespace Rezolver.Tests.ConfigurationTests.Json
{
	[TestClass]
	public class JsonConfigurationTests : JsonConfigurationTestsBase
	{
		//TODO: flesh out these tests a bit more.  Need to be far more granular and focused.

		//at the moment I've just created a file with some cool stuff in it and testing that it works.

		[TestMethod]
		public void ShouldCreateJsonConfigurationFromJsonString()
		{
			var parser = new JsonConfigurationParser();

			IConfiguration configuration = parser.Parse(JsonTestResources.Simple);

			Assert.IsInstanceOfType(configuration, typeof(JsonConfiguration));
		}

		[TestMethod]
		public void AdapterShouldBuildRezolverBuilder()
		{
			var parser = new JsonConfigurationParser();

			IConfiguration configuration = parser.Parse(JsonTestResources.Simple);
			//use the defaul adapter
			IConfigurationAdapter adapter = new ConfigurationAdapter();
			var builder = adapter.CreateBuilder(configuration);

			Assert.IsInstanceOfType(builder, typeof(IRezolverBuilder));

			var rezolver = new DefaultRezolver(builder, new AssemblyRezolveTargetCompiler());
			var str = rezolver.Resolve<string>();
			Assert.AreEqual("Hello world", str);
			var en = rezolver.Resolve<IEnumerable<int>>();
			Assert.IsNotNull(en);
			Assert.IsTrue(en.SequenceEqual(new[] { 1, 2, 3 }));

		}
	}
}
