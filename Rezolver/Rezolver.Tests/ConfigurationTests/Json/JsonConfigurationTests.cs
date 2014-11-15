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
		public void ShouldCreateJsonConverterFromJsonString()
		{
			var parser = new JsonConfigurationParser();

			IConfiguration configuration = parser.Parse(JsonTestResources.Simple);

			Assert.IsInstanceOfType(configuration, typeof(JsonConfiguration));
		}
	}
}
