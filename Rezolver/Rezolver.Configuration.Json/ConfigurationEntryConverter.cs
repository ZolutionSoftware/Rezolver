using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Configuration;

namespace Rezolver.Configuration.Json
{
	public class ConfigurationEntryConverter : JsonConverter
	{
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}
		public override bool CanConvert(Type objectType)
		{
			return typeof(IConfigurationEntry) == objectType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			IJsonLineInfo lineInfo = (reader as IJsonLineInfo) ?? StubJsonLineInfo.Instance;
			IConfigurationEntry toReturn = null;
			if (reader.TokenType == JsonToken.StartObject)
			{
				if (!reader.Read())
					throw new JsonConfigurationException("End of file before reading property name", reader);
				if (reader.TokenType != JsonToken.PropertyName)
					throw new JsonConfigurationException(JsonToken.PropertyName, reader);

				//depending on the value we change what we create
				//'type' will generate a more complex type name object,
				//anything else will be treated as a straight type name.
				string propName = reader.Value as string;

				if (string.IsNullOrWhiteSpace(propName))
					throw new JsonConfigurationException("Property name cannot be null, empty or whitespace", reader);

				toReturn = LoadTypeRegistrationEntry(reader, serializer, lineInfo, propName);
			}
			else
				throw new JsonConfigurationException(JsonToken.StartObject, reader);

			return toReturn;
		}

		ITypeRegistrationEntry LoadTypeRegistrationEntry(JsonReader reader, JsonSerializer serializer, IJsonLineInfo lineInfo, string propName = null)
		{
			if (propName == null) propName = reader.Value as string;

			var startPos = lineInfo.Capture();

			ITypeReference[] regTypes = null;
			bool expectValueProperty = false;
			if ("type".Equals(propName, StringComparison.OrdinalIgnoreCase))
			{
				//move to the content
				reader.Read();
				regTypes = new[] { serializer.Deserialize<TypeReference>(reader) };
				expectValueProperty = true;
			}
			else if ("types".Equals(propName, StringComparison.OrdinalIgnoreCase))
			{
				//move to the content
				reader.Read();
				regTypes = serializer.Deserialize<TypeReference[]>(reader);
				expectValueProperty = true;
			}
			else
			{
				regTypes = new[] { new TypeReference(propName) };
				if (!reader.Read())
					throw new JsonConfigurationException(lineInfo.FormatMessageForThisLine("End of file before entry target property value"), reader);
			}

			if(expectValueProperty)
			{
				reader.Read();
				if (reader.TokenType != JsonToken.PropertyName)
					throw new JsonConfigurationException(JsonToken.PropertyName, reader);
				if (!"value".Equals(reader.Value))
					throw new JsonConfigurationException(string.Format("Unexpected property '{0}', expected 'value'", reader.Value as string), reader);
				reader.Read();
			}

			RezolveTargetMetadataWrapper wrappedMeta = serializer.Deserialize<RezolveTargetMetadataWrapper>(reader);
			//and then unwrap the meta data for the target types
			return new TypeRegistrationEntry(regTypes, wrappedMeta.UnwrapMetadata(regTypes), startPos.ToConfigurationLineInfo(lineInfo));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
