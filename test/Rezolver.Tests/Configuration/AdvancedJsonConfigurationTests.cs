using Newtonsoft.Json.Linq;
using Rezolver.Configuration;
using Rezolver.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Configuration
{

  public class AdvancedJsonConfigurationTests : JsonConfigurationTestsBase
  {
#if !DOTNET
		protected override IConfigurationAdapter CreateAdapter()
		{
			return new ConfigurationAdapter(AdvancedConfigurationAdapterContextFactory.Instance);
		}

		protected override string PreProcess(string json)
		{
			var config = JObject.Parse(json);
			if (config.Remove("assemblies"))
			{
				Console.WriteLine("Removed 'assemblies' property from configuration json for advanced test");
				return config.ToString();
			}

			return base.PreProcess(json);
		}

		[Fact]
		public void AdapterShouldBuildRezolverBuilder()
		{
			var json = @"{
	""rezolve"": [
		{ ""System.Int32"": 10 },
		{ ""Rezolver.Tests.ConfigurationTests.RequiresInt"": { ""$construct"": ""$auto"" }
			},
		{ ""Rezolver.Tests.ConfigurationTests.IRequiresInt"": { ""$construct"": ""Rezolver.Tests.ConfigurationTests.RequiresInt"" }
			},
		{
				""type"": { ""name"": ""System.Collections.Generic.IEnumerable"", ""args"": [ ""System.Int32"" ]
	},
			""value"": [ 1, 2, 3 ]
},
		{
			""types"": [ ""System.Object"", ""System.String"" ],
			""value"": ""Hello world""
		}
	]
}";
			var parser = new JsonConfigurationParser();
			IConfiguration configuration = parser.Parse(json);
			//use the defaul adapter
			IConfigurationAdapter adapter = new ConfigurationAdapter(AdvancedConfigurationAdapterContextFactory.Instance);
			var builder = adapter.CreateBuilder(configuration);

			Assert.IsType<Builder>(builder);

			var rezolver = new Container(builder, new TargetAssemblyCompiler());
			var str = rezolver.Resolve<string>();
			Assert.Equal("Hello world", str);
			var en = rezolver.Resolve<IEnumerable<int>>();
			Assert.NotNull(en);
			Assert.True(en.SequenceEqual(new[] { 1, 2, 3 }));

		}
#endif
  }
}
