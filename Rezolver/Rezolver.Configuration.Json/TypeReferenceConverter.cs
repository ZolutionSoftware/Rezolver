using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	/// <summary>
	/// Responsible for converting Json to a Rezolver Configuration TypeReference
	/// </summary>
	public class TypeReferenceConverter : JsonConverter
	{
		/// <summary>
		/// Special class used by the TypeReferenceConverter as an intermediary for reading type references from Json.
		/// </summary>
		private class JsonTypeReference
		{
			[JsonProperty("type")]
			public string TypeName
			{
				get;
				set;
			}

			[JsonProperty("args")]
			public TypeReference[] GenericArguments
			{
				get;
				set;
			}
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
			return typeof(TypeReference) == objectType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.String)
			{
				return new TypeReference(reader.Value as string);
			}
			else if (reader.TokenType == JsonToken.StartObject)
			{
				var temp = new JsonTypeReference();
				serializer.Populate(reader, temp);
				return new TypeReference(temp.TypeName, temp.GenericArguments);
			}
			else throw new InvalidOperationException(((reader as IJsonLineInfo) ?? StubJsonLineInfo.Instance).FormatMessageForThisLine("Invalid Type Reference"));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
