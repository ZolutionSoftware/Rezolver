using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rezolver.Configuration.Json;
using Rezolver.Configuration;

namespace Rezolver.Tests.ConfigurationTests.Json
{
	/// <summary>
	/// Base test class for JSON configuration.
	/// 
	/// Contains tests that should work regardless of the adapter context factory being used.
	/// </summary>
	public class JsonConfigurationTestsBase
	{
		protected DefaultRezolver ParseConfigurationAndBuild(string json)
		{
			JsonConfigurationParser parser = CreateParser();
			var parsed = parser.Parse(json);
			var adapter = CreateAdapter();
			var builder = adapter.CreateBuilder(parsed);
			var rezolver = CreateRezolver(builder);
			return rezolver;
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
		
	}
}
