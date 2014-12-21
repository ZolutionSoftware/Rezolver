using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration.Json;
using Rezolver.Configuration;
using System.Collections.Generic;

namespace Rezolver.Tests.ConfigurationTests.Json
{
	[TestClass]
	public class AdvancedJsonConfigurationTests : JsonConfigurationTestsBase
	{
		protected override IConfigurationAdapter CreateAdapter()
		{
			return new ConfigurationAdapter(AdvancedConfigurationAdapterContextFactory.Instance);
		}

		[TestMethod]
		public void AdapterShouldBuildRezolverBuilder()
		{
			var parser = new JsonConfigurationParser();
			IConfiguration configuration = parser.Parse(JsonTestResources.Advanced);
			//use the defaul adapter
			IConfigurationAdapter adapter = new ConfigurationAdapter(AdvancedConfigurationAdapterContextFactory.Instance);
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
