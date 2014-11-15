using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	public class JsonConfigurationParser
	{
		public virtual JsonSerializer CreateSerializer(JsonSerializerSettings settings)
		{
			return JsonSerializer.Create(settings);
		}

		public virtual JsonSerializerSettings CreateJsonSerializerSettings()
		{
			var toReturn = (JsonConvert.DefaultSettings ?? (() => new JsonSerializerSettings()))();
			toReturn.Converters.Add(new TypeReferenceConverter());
			toReturn.Converters.Add(new ConfigurationEntryConverter());
			return toReturn;
		}

		public IConfiguration Parse(string json)
		{
			return Parse(json, CreateSerializer(CreateJsonSerializerSettings()));
		}

		public virtual IConfiguration Parse(string json, JsonSerializer jsonSerializer)
		{
			if (string.IsNullOrWhiteSpace(json))
				throw new ArgumentException("Argument cannot be a null, empty or whitespace string", "json");
			if (jsonSerializer == null)
				throw new ArgumentException("jsonSerializer");
			using (var sr = new StringReader(json))
			{
				using (var reader = new JsonTextReader(sr))
				{
					return jsonSerializer.Deserialize<JsonConfiguration>(reader);
				}
			}
		}
	}
}
