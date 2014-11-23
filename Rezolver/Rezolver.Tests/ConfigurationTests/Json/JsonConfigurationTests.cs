using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration;
using Rezolver.Configuration.Json;

namespace Rezolver.Tests.ConfigurationTests.Json
{
	[TestClass]
	public class JsonConfigurationTests
	{
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
		}
	}
}
