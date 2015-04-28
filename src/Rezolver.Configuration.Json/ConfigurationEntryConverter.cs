using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Configuration;

namespace Rezolver.Configuration.Json
{
	/// <summary>
	/// JSON converter for IConfigurationEntry
	/// 
	/// The converter defaults to looking for type registrations; to enable it to look for a different entry type,
	/// you must instruct Json.Net to create an instance of it using the constructor that accepts a ConfigurationEntryType
	/// </summary>
	public class ConfigurationEntryConverter : JsonConverter
	{
		private readonly ConfigurationEntryType _expectedType;
		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public ConfigurationEntryConverter()
			: this(ConfigurationEntryType.TypeRegistration)
		{

		}

		public ConfigurationEntryConverter(ConfigurationEntryType expectedType)
		{
			_expectedType = expectedType;
		}

		public override bool CanConvert(Type objectType)
		{
			switch(_expectedType)
			{
				case ConfigurationEntryType.TypeRegistration:
					return typeof(ITypeRegistrationEntry) == objectType || typeof(IConfigurationEntry) == objectType;
				case ConfigurationEntryType.AssemblyReference:
					return typeof(IAssemblyReferenceEntry) == objectType || typeof(IConfigurationEntry) == objectType;
				case ConfigurationEntryType.Extension:
					return typeof(IConfigurationExtensionEntry) == objectType || typeof(IConfigurationEntry) == objectType;
				default:
					return false;
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			IJsonLineInfo lineInfo = (reader as IJsonLineInfo) ?? StubJsonLineInfo.Instance;
			switch(_expectedType)
			{
				case ConfigurationEntryType.TypeRegistration:
					return LoadTypeRegistrationEntry(reader, serializer, lineInfo);
				case ConfigurationEntryType.AssemblyReference:
					return LoadAssemblyReferenceEntry(reader, serializer, lineInfo);
				default:
					throw new NotSupportedException(string.Format("Cannot currently deserialise an entry type of {0} from JSON", _expectedType));
			}
		}

		IConfigurationEntry LoadAssemblyReferenceEntry(JsonReader reader, JsonSerializer serializer, IJsonLineInfo lineInfo)
		{
			IJsonLineInfo startPos = lineInfo.Capture();
			
			if (reader.TokenType != JsonToken.String)
				throw new JsonConfigurationException(JsonToken.String, reader);
			string assemblyName = reader.Value as string;
			//reader.Read();
			try{
				return new AssemblyReferenceEntry(assemblyName, startPos.ToConfigurationLineInfo(lineInfo));
			}
			catch(ArgumentException)
			{
				throw new JsonConfigurationException("The assembly name is not valid", reader);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="serializer"></param>
		/// <param name="startPos">Position at which the type registration entry commence parsing</param>
		/// <param name="lineInfo">Provides access to the current line/column position that the reader is at</param>
		/// <param name="propName"></param>
		/// <returns></returns>
		IConfigurationEntry LoadTypeRegistrationEntry(JsonReader reader, JsonSerializer serializer, IJsonLineInfo lineInfo)
		{
			IJsonLineInfo startPos = lineInfo.Capture();
			if (reader.TokenType != JsonToken.StartObject)
				throw new JsonConfigurationException(JsonToken.StartObject, reader);

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

			if (propName == null) propName = reader.Value as string;
			ITypeReference[] regTypes = null;
			bool expectValueProperty = false;
			bool isMultipleRegistration = false;
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
				//read ahead so we know where the type reference ends
				if (!reader.Read())
					throw new JsonConfigurationException(lineInfo.FormatMessageForThisLine("End of file before entry target property value"), reader);
				regTypes = new[] { new TypeReference(propName, startPos.ToConfigurationLineInfo(lineInfo)) };
			}

			if (expectValueProperty)
			{
				reader.Read();
				if (reader.TokenType != JsonToken.PropertyName)
					throw new JsonConfigurationException(JsonToken.PropertyName, reader);
				if (!"value".Equals(reader.Value))
					throw new JsonConfigurationException(string.Format("Unexpected property '{0}', expected 'value'", reader.Value as string), reader);
				reader.Read();
			}

			IRezolveTargetMetadata meta = serializer.Deserialize<IRezolveTargetMetadata>(reader);
			//and then unwrap the meta data for the target types
			var unwrapped = meta.Bind(regTypes);

			//if we get a metadata list, then the configuration is instructing us to do a multiple registration.
			//that is, a multi registration of type T will mean that we will later resolve IEnumerable<T>
			//to register an array directly we will need a new metadata type of 'array' (or similar) which will then 
			//require a MetadataList inside it so that we can push that logic one step further away.  That said, we'd also
			//need a new rezolvetarget for that.
			if (unwrapped.Type == RezolveTargetMetadataType.MetadataList)
				isMultipleRegistration = true;

			return new TypeRegistrationEntry(regTypes, unwrapped, isMultipleRegistration, startPos.ToConfigurationLineInfo(lineInfo));
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
