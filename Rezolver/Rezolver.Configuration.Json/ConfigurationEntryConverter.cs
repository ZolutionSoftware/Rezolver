using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
					throw new InvalidOperationException(lineInfo.FormatMessageForThisLine("End of file before reading property name"));
				if (reader.TokenType != JsonToken.PropertyName)
					throw new InvalidOperationException(lineInfo.FormatMessageForThisLine("Unexpected token type '{0}', expected '{1}'", reader.TokenType, JsonToken.PropertyName));

				//depending on the value we change what we create
				//'type' will generate a more complex type name object,
				//anything else will be treated as a straight type name.
				string propName = reader.Value as string;

				if (string.IsNullOrWhiteSpace(propName))
					throw new InvalidOperationException(lineInfo.FormatMessageForThisLine("Property name cannot be null, empty or whitespace"));

				//determine the type of nodes that we have here
				if (propName.StartsWith("$"))
				{
					reader.Read();
					reader.Skip();
					return null;
				}
				else
				{
					toReturn = LoadTypeRegistrationEntry(reader, serializer, lineInfo, propName);
				}
			}
			else
				throw new InvalidOperationException(lineInfo.FormatMessageForThisLine("Unexpected token type '{0}', expected '{1}'", reader.TokenType, JsonToken.StartObject));

			if (reader.TokenType == JsonToken.EndObject)
				reader.Read();
			return toReturn;
		}

		ITypeRegistrationEntry LoadTypeRegistrationEntry(JsonReader reader, JsonSerializer serializer, IJsonLineInfo lineInfo, string propName = null)
		{
			if (propName == null) propName = reader.Value as string;

			var startPos = lineInfo.Capture();

			ITypeReference[] targetTypes = null;

			if ("type".Equals(propName, StringComparison.OrdinalIgnoreCase))
			{
				//move to the content
				reader.Read();
				targetTypes = new[] { serializer.Deserialize<TypeReference>(reader) };
			}
			else if ("types".Equals(propName, StringComparison.OrdinalIgnoreCase))
			{
				reader.Read();
				targetTypes = serializer.Deserialize<TypeReference[]>(reader);
			}
			else
			{
				targetTypes = new[] { new TypeReference(propName) };
			}

			if (!reader.Read())
				throw new InvalidOperationException(lineInfo.FormatMessageForThisLine("End of file before reading property value"));

			IRezolveTargetMetadata meta = null;

			switch (reader.TokenType)
			{
				//in the case of simple types, we bake them as objects for an object target
				case JsonToken.Boolean:
				case JsonToken.Bytes:
				case JsonToken.Date:
				case JsonToken.Float:
				case JsonToken.Integer:
				case JsonToken.String:
					{
						meta = new ObjectTargetMetadata(reader.Value, reader.ValueType);
						reader.Read();
						break;
					}
				case JsonToken.StartObject:
					{
						var o = JObject.ReadFrom(reader);
						break;
					}
			}

			return new TypeRegistrationEntry(targetTypes, meta, startPos.ToConfigurationLineInfo(lineInfo));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
