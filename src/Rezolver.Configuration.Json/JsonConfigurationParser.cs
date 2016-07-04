// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
			//using special converters to handle the loading of the Json.  They are mapped to interface
			//types so that we can branch the serialization code to create specialised versions of 
			//configuration entries or target metadata depending on the content of the JSON.
			toReturn.Converters.Add(new TypeReferenceConverter());
			toReturn.Converters.Add(new ConfigurationEntryConverter(ConfigurationEntryType.TypeRegistration));
			toReturn.Converters.Add(new ConfigurationEntryConverter(ConfigurationEntryType.AssemblyReference));
			toReturn.Converters.Add(new RezolveTargetMetadataConverter());
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
