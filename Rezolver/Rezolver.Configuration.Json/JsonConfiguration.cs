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

				return new JsonConfiguration(proxy.RezolveEntries);
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				throw new NotImplementedException();
			}
		}
	}
}
