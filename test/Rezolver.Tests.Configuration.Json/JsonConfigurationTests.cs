using Rezolver.Configuration;
using Rezolver.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Configuration.Json
{
  public class JsonConfigurationTests : JsonConfigurationTestsBase
  {
    [Fact]
    public void ShouldCreateJsonConfigurationFromJsonString()
    {
      string json = @"{
	""assemblies"": [
		""Rezolver.Tests.Configuration.Json""
	],
	""rezolve"": [
		{ ""System.Int32"": 10 },
		{ ""Rezolver.Tests.Types.RequiresInt"": { ""$construct"": ""$auto"" } },
		{ ""Rezolver.Tests.Types.IRequiresInt"": { ""$construct"": ""Rezolver.Tests.Types.RequiresInt"" } },
		{
			""type"": { ""name"": ""System.Collections.Generic.IEnumerable"", ""args"": [ ""System.Int32"" ] },
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
      Assert.IsType<JsonConfiguration>(configuration);
    }


    [Fact]
    public void AdapterShouldBuildRezolverBuilder()
    {
      string json = @"{
	""assemblies"": [
		""Rezolver.Tests.Configuration.Json""
	],
	""rezolve"": [
		{ ""System.Int32"": 10 },
		{ ""Rezolver.Tests.Types.RequiresInt"": { ""$construct"": ""$auto"" } },
		{ ""Rezolver.Tests.Types.IRequiresInt"": { ""$construct"": ""Rezolver.Tests.Types.RequiresInt"" } },
		{
			""type"": { ""name"": ""System.Collections.Generic.IEnumerable"", ""args"": [ ""System.Int32"" ] },
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
      IConfigurationAdapter adapter = new ConfigurationAdapter();
      var builder = adapter.CreateTargetContainer(configuration);

      Assert.IsType<TargetContainer>(builder);

      var container = new Container(builder);
      var str = container.Resolve<string>();
      Assert.Equal("Hello world", str);
      var en = container.Resolve<IEnumerable<int>>();
      Assert.NotNull(en);
      Assert.True(en.SequenceEqual(new[] { 1, 2, 3 }));
    }
  }
}
