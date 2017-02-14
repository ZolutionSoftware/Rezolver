using Rezolver.Configuration;
using Rezolver.Configuration.Json;
using Rezolver.Tests.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Rezolver.Tests.Configuration.Json
{
  /// <summary>
  /// Base test class for JSON configuration.
  /// 
  /// Contains tests that should work regardless of the adapter context factory being used.
  /// </summary>
  public abstract class JsonConfigurationTestsBase
  {
    protected Container ParseConfigurationAndBuild(string json, [CallerMemberName]string testName = null)
    {
      JsonConfigurationParser parser = CreateParser();
      //gives us the chance to hack the JSON string for derived tests.
      //for the advanced tests, for example, we always remove the assemblies folder
      json = PreProcess(json);
      Console.WriteLine("Attempting to parse JSON configuration for test {0}.{1}:", GetType(), testName ?? "[Unknown]");
      Console.WriteLine(json);
      var parsed = parser.Parse(json);
      var adapter = CreateAdapter();
      var builder = adapter.CreateTargetContainer(parsed);
      var container = CreateContainer(builder);
      return container;
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

    protected virtual Container CreateContainer(ITargetContainer builder)
    {
      return new Container(builder);
    }

    [Fact]
    public void ShouldRezolveInt()
    {
      string json = @"
	{
		""rezolve"": [
			{ ""System.Int32"": 1 }
		] 
	}
";
      var container = ParseConfigurationAndBuild(json);
      int result = container.Resolve<int>();

      Assert.Equal(1, result);
    }

    [Fact]
    public void ShouldRezolveString()
    {
      string json = @"
{
	""rezolve"": [
		{ ""System.String"": ""Hello World!"" }
	]
}
";

      var container = ParseConfigurationAndBuild(json);
      string result = container.Resolve<string>();
      Assert.Equal("Hello World!", result);
    }

    [Fact]
    public void ShouldRezolveInstanceCountingType_1()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //first way of doing type references (as per last two types) for the registration - 
        //use the type name as the property name, with it's value being the target (the { "$construct" : ... } bit)
        string json = @"
{
	""assemblies"": [ ""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"" : [
		{ ""Rezolver.Tests.Types.InstanceCountingType"" : { ""$construct"" : ""$auto"" } }
	]
}";
        var container = ParseConfigurationAndBuild(json);
        var instance = container.Resolve<InstanceCountingType>();
        Assert.Equal(session.InitialInstanceCount + 1, InstanceCountingType.InstanceCount);
      }
    }


    [Fact]
    public void ShouldRezolveInstanceCountingType_2()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //second way of doing this - an explicit 'type' member with a string value, 
        //and a value member with the target object

        //note - this is just another way of specifying a type reference in a json file by string.
        string json = @"
{
	""assemblies"": [ ""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"" : [
		{
			""type"": ""Rezolver.Tests.Types.InstanceCountingType"",
			""value"": { ""$construct"" : ""$auto"" } 
		}
	]
}";

        var container = ParseConfigurationAndBuild(json);
        var instance = container.Resolve<InstanceCountingType>();
        Assert.Equal(session.InitialInstanceCount + 1, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void ShouldRezolveInstanceCountingType_3()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //third way of doing this - an explicit type member, with the value being an object that
        //describes the type reference with a base type name and potentially generic arguments.
        //this is the most verbose way to provide a type reference in a json configuration file, and
        //can be used both to describe the type against which a target will be registered, but also
        //the type to build, e.g. as the value for the $construct property of a target.
        string json = @"
{
	""assemblies"": [ ""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"" : [
		{
			""type"": { ""name"": ""Rezolver.Tests.Types.InstanceCountingType"" },
			""value"": { ""$construct"" : ""$auto"" } 
		}
	]
}";

        var container = ParseConfigurationAndBuild(json);
        var instance = container.Resolve<InstanceCountingType>();
        Assert.Equal(session.InitialInstanceCount + 1, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void ShouldRezolveRequiresInt()
    {
      //I'm not doing the same three examples as above here - that's already been tested.
      //however I am registering an entry for IRequiresInt, but building an instance of RequiresInt
      string json = @"
{
	""assemblies"":[""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"": [
		{ ""System.Int32"": 105 },
		{ ""Rezolver.Tests.Types.IRequiresInt"" : { ""$construct"": ""Rezolver.Tests.Types.RequiresInt"" } }
	]
}
";
      var container = ParseConfigurationAndBuild(json);
      IRequiresInt requiresInt = container.Resolve<IRequiresInt>();
      Assert.Equal(105, requiresInt.IntValue);
    }

    [Fact]
    public void ShouldRezolveRequiresIntByTwoTypes()
    {
      //this time registering RequiresInt against both RequiresInt type and the interface
      //notice this time the use of 'types' instead of 'type'.  This accepts an array of type references which,
      //as mentioned in an earlier test, can be a literal string specifying a type name, or a construct such 
      //as { "name": "[typename]", "args": [ {type_reference}, ... ] }
      string json = @"{
	""assemblies"":[""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"": [
		{ ""System.Int32"": 110 },
		{ 
			""types"": [ ""Rezolver.Tests.Types.IRequiresInt"", ""Rezolver.Tests.Types.RequiresInt"" ],
			""value"": { ""$construct"": ""Rezolver.Tests.Types.RequiresInt"" }
		}
	]
}";

      var container = ParseConfigurationAndBuild(json);
      IRequiresInt requiresInt = container.Resolve<IRequiresInt>();
      RequiresInt requiresInt2 = container.Resolve<RequiresInt>();

      Assert.Equal(requiresInt.IntValue, requiresInt2.IntValue);
      Assert.Equal(110, requiresInt.IntValue);
      //however, they shouldn't be the same instance:
      Assert.NotSame(requiresInt, requiresInt2);
    }

    [Fact]
    public void ShouldRezolveSingletonRequiresIntByTwoTypes()
    {
      //TODO: implement aliasing as an extension method on the IRezolveTargetContainerExtensions class - RegisterAlias
      //What will it do?
      //It will take a target type and a registration type - and it will create a ResolvedTarget that rezolves an instance of the target type,
      //but then register it against the registration type.
      //Why do we need it?
      //In this test (and the other on that currently fails), the intention is to register a singleton once for multiple types, ensuring that only
      //one instance is shared between all target types.  This cannot currently be done because the singleton (rightly) creates a single instance for
      //each type that it receives a request for.  By implementing aliasing, we can use the same singleton for multiple types, but only one of those
      //registrations will point to that singleton.  All the others will represents a recursion back into the container to resolve against that one
      //singleton.
      //After doing that, what next?
      //Head over to RegisterInstruction.cs in the configuration project and use the new extension method for hierarchies of related types (from line 57).
      //There is no need to implement this logic anywhere else, as people using the builder directly can choose to use alising or not.  It's really for
      //the configuration scenario.

      //as you can guess by the last assert in the previous test - this does the same
      //again, except this time, it's expected that we get the same instance for both rezolve calls.
      //singletons are easy - just take the value entry that you typically put, and wrap it in a { "$singleton": /* original value */ }
      string json = @"{
	""assemblies"":[""Rezolver.Tests.Configuration.Json"" ],
	""rezolve"": [
		{ ""System.Int32"": 115 },
		{ 
			""types"": [ ""Rezolver.Tests.Types.IRequiresInt"", ""Rezolver.Tests.Types.RequiresInt"" ],
			""value"": { ""$singleton"" : { ""$construct"": ""Rezolver.Tests.Types.RequiresInt"" } }
		}
	]
}";

      var container = ParseConfigurationAndBuild(json);
      IRequiresInt requiresInt = container.Resolve<IRequiresInt>();
      RequiresInt requiresInt2 = container.Resolve<RequiresInt>();

      Assert.Equal(115, requiresInt.IntValue);
      Assert.Same(requiresInt, requiresInt2);
    }

    [Fact]
    public void ShouldRezolveArrayOfStrings()
    {
      //here, an array of strings can be provided by a literal string array in the JSON.

      string json = @"{
	""rezolve"" : [
		{ ""System.String[]"" : [ ""Hello World0"", ""Hello World1"", ""Hello World2"" ] }
	]
}";
      var container = ParseConfigurationAndBuild(json);
      string[] array = container.Resolve<string[]>();
      Assert.NotNull(array);
      Assert.Equal(3, array.Length);
      Assert.True(Enumerable.Range(0, 3).Select(i => string.Format("Hello World{0}", i)).SequenceEqual(array));
    }

    [Fact]
    public void ShouldRezolveEnumerableOfStringsViaMultiRegistration()
    {
      //so instead of registering an array - we're registering a bunch of strings as a multiple registration
      //this then means they are retrievable as an IEnumerable<string>
      string json = @"{
	""rezolve"" : [
		{ 
			""System.String"" : {
				""$targets"": [ ""Hello Multiple0"", ""Hello Multiple1"", ""Hello Multiple2"" ] 
			}
		}
	]
}";
      var container = ParseConfigurationAndBuild(json);
      IEnumerable<string> multiple = container.Resolve<IEnumerable<string>>();
      Assert.NotNull(multiple);
      var array = multiple.ToArray();
      Assert.Equal(3, array.Length); ;
      Assert.True(Enumerable.Range(0, 3).Select(i => string.Format("Hello Multiple{0}", i)).SequenceEqual(array));
    }

    [Fact]
    public void ShouldRezolveEnumerableOfInstanceCountingTypeViaMultiRegistration()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //there's a way of doing this which binds to the array/enumerable type or whatever, and then 
        //create the underlying array inline within JSON - however it's likely to require implementing
        //JSON serialization.
        //this test shows instead how we specify element type, instruct the parser that we intend to register
        //multiple targets against that, and then the caller will simply request IEnumerable<type>
        string json = @"{
	""assemblies"":[ ""Rezolver.Tests.Configuration.Json"" ],	
	""rezolve"" : [
		{
			""Rezolver.Tests.Types.InstanceCountingType"" :
			{ 
				""$targets"" :
				[ 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }, 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }, 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }
				]
			} 
		}
	]
}";
        var container = ParseConfigurationAndBuild(json);
        IEnumerable<InstanceCountingType> multiple = container.Resolve<IEnumerable<InstanceCountingType>>();
        Assert.NotNull(multiple);
        var array = multiple.ToArray();
        Assert.Equal(3, array.Length);
        Assert.Equal(session.InitialInstanceCount + 3, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void ShouldRezolveEnumerableOfStringViaDirectRegistration()
    {
      //bakes a literal string array as a target for enumerable of strings.
      string json = @"{
	""rezolve"" : [
		{ 
			""type"" : { ""name"" : ""System.Collections.Generic.IEnumerable"", ""args"" : [ ""System.String"" ] },
			""value"": [ ""Hello Generic0"", ""Hello Generic1"", ""Hello Generic2"" ] 
		}
	]
}";
      var container = ParseConfigurationAndBuild(json);
      IEnumerable<string> strings = container.Resolve<IEnumerable<string>>();
      Assert.NotNull(strings);
      var array = strings.ToArray();
      Assert.Equal(3, array.Length);
      Assert.True(Enumerable.Range(0, 3).Select(i => string.Format("Hello Generic{0}", i)).SequenceEqual(array));
    }

    [Fact]
    public void ShouldRezolveArrayOfInstanceCountingTypeViaDirectRegistration()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        string json = @"{
	""assemblies"":[ ""Rezolver.Tests.Configuration.Json"" ],	
	""rezolve"" : [
		{
			""type"" : { ""name"" : ""Rezolver.Tests.Types.InstanceCountingType"", ""array"": true },
			""value"" : 
			{ 
				""$array"" : ""Rezolver.Tests.Types.InstanceCountingType"",
				""values"" : 
				[ 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }, 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }, 
					{ ""$construct"" : ""Rezolver.Tests.Types.InstanceCountingType""  }
				]
			} 
		}
	]
}";
        var container = ParseConfigurationAndBuild(json);
        InstanceCountingType[] result = container.Resolve<InstanceCountingType[]>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal(session.InitialInstanceCount + 3, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void ShouldRezolveArrayOfInstanceCountingTypeUsingAutoViaDirectRegistration()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //demonstrates how you can use the $auto typename in a $construct call to inherit the 
        ///type name from the array's element type.
        string json = @"{
	""assemblies"":[ ""Rezolver.Tests.Configuration.Json"" ],	
	""rezolve"" : [
		{
			""type"" : { ""name"" : ""Rezolver.Tests.Types.InstanceCountingType"", ""array"": true },
			""value"" : 
			{ 
				""$array"" : ""Rezolver.Tests.Types.InstanceCountingType"",
				""values"" : 
				[ 
					{ ""$construct"" : ""$auto""  }, 
					{ ""$construct"" : ""$auto""  }, 
					{ ""$construct"" : ""$auto""  }
				]
			} 
		}
	]
}";
        var container = ParseConfigurationAndBuild(json);
        InstanceCountingType[] result = container.Resolve<InstanceCountingType[]>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal(session.InitialInstanceCount + 3, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void ShouldRezolveArrayOfInstanceCountingTypeUsingAuto2ViaDirectRegistration()
    {
      using (var session = InstanceCountingType.NewSession())
      {
        //demonstrates how you can use the $auto typename, first in the $array property to inherit
        //the element type of an explicit array type specified as the type of a registration. 
        //and then again in the $construct call to inherit the type name from the array's element type.

        //note it's imperative that the type of the registration is explicitly marked as 'array = true',
        //so do not use 'Type[]'
        string json = @"{
	""assemblies"":[ ""Rezolver.Tests.Configuration.Json"" ],	
	""rezolve"" : [
		{
			""type"" : { ""name"" : ""Rezolver.Tests.Types.InstanceCountingType"", ""array"": true },
			""value"" : 
			{ 
				""$array"" : ""$auto"",
				""values"" : 
				[ 
					{ ""$construct"" : ""$auto""  }, 
					{ ""$construct"" : ""$auto""  }, 
					{ ""$construct"" : ""$auto""  }
				]
			} 
		}
	]
}";
        var container = ParseConfigurationAndBuild(json);
        InstanceCountingType[] result = container.Resolve<InstanceCountingType[]>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.Equal(session.InitialInstanceCount + 3, InstanceCountingType.InstanceCount);
      }
    }

    [Fact]
    public void BindConstructorOfNetworkCredentials()
    {
      //binding a constructor explicitly - using a type from the .Net framework
      string json = @"{
			""assemblies"": [""System.Net.Primitives""],
			""rezolve"" : [ 
			{
				""System.Net.NetworkCredential"": {
					""$construct"": ""$auto"",
					""$args"": {
						""$sig"": [ ""System.String"", ""System.String"" ],
						""userName"": ""username"",
						""password"": ""password""
					}
				}
			}
		 ]}";
      var container = ParseConfigurationAndBuild(json);
      var result = container.Resolve<System.Net.NetworkCredential>();
      Assert.Equal("username", result.UserName);
      Assert.Equal("password", result.Password);
    }
  }
}
