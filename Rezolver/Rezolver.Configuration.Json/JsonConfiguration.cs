using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	[JsonConverter(typeof(JsonConfigurationConverter))]
	public class JsonConfiguration : IConfiguration
	{
		/// <summary>
		/// The typename to use on a type entry when you want to register a constructor target directly for the type that is registered.
		/// E.g. { 'MyNamespace.Foo' : { 'type' : "$self' } }
		/// Which registers MyNamespace.Foo with a constructor target that binds to the same type.
		/// </summary>
		public const string AutoConstructorType = "$self";

		public string FileName
		{
			get;
			private set;
		}

		public IEnumerable<IConfigurationEntry> Entries
		{
			get;
			private set;
		}
		private JsonConfiguration(IEnumerable<IConfigurationEntry> entries)
		{
			Entries = entries;
		}

		public class JsonConfigurationConverter : JsonConverter
		{
			private class ConfigurationProxy
			{
				[JsonProperty("rezolve")]
				public IConfigurationEntry[] RezolveEntries { get; set; }

				/// <summary>
				/// Note this property has an explicit JsonConverter to tell the serializer to deserialize assembly references.
				/// </summary>
				[JsonProperty("assemblies")]
				public IAssemblyReferenceEntry[] Assemblies { get; set; }
				[JsonProperty("using")]
				public IConfigurationEntry[] NamespaceImports { get; set; }
			}

			public override bool CanWrite
			{
				get
				{
					return false;
				}
			}
			public override bool CanConvert(Type objectType)
			{
				return typeof(JsonConfiguration).IsAssignableFrom(objectType);
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var proxy = serializer.Deserialize<ConfigurationProxy>(reader);

				return new JsonConfiguration(proxy.RezolveEntries.Concat(proxy.Assemblies).Concat(proxy.NamespaceImports).ToArray());
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}
}
