using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	/// <summary>
	/// This converter is registered against the type RezolveTargetMetadataWrapper 
	/// </summary>
	public class RezolveTargetMetadataConverter : JsonConverter
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
			return typeof(RezolveTargetMetadataWrapper).Equals(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
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
				case JsonToken.StartArray:
					{
						JArray a = JArray.ReadFrom(reader) as JArray;

						//create an object target metadata that will create an object on demand from the 
						//array
						meta = CreateDeferredJsonDeserializerTarget(a, serializer);
						reader.Read();
						break;
					}
				case JsonToken.StartObject:
					{
						JObject o = JObject.ReadFrom(reader) as JObject;

						if (o == null)
							throw new JsonConfigurationException("Invalid JSON object", reader);

						//create a JObject first, analyse the contents and then build the target meta
						//if it has a 'target' member, then it's a constructor target
						meta = LoadTargetMetadata(o, serializer);
						reader.Read();
						break;
					}
				case JsonToken.Comment:
					{
						reader.Read();
						break;
					}
				default:
					throw new JsonConfigurationException(string.Format("Json Token Type {0} is not supported here", reader.TokenType), reader);
			}

			//we wrap the target because we have to allow rewriting targets after they're deserialized in isolation.
			//constructor targets - for example - support the '$self' type name, but because the types being registered are not
			//known when deserialising the target, we have to have a second-shot
			return new RezolveTargetMetadataWrapper(meta);
		}

		private IRezolveTargetMetadata LoadTargetMetadata(JObject jObject/*, ITypeReference[] regTypes*/, JsonSerializer serializer)
		{
			//if (!jObject.HasValues)
			//	throw new JsonConfigurationException("An empty object is not allowed here", jObject);

			JObject tempChildObject = jObject["$scopedSingleton"] as JObject;

			//by default, singletons are baked using '{ singleton : { target } }'
			//so we look for a property called either, and then use its actual value (which MUST be an object)
			//as the inner object.
			if (tempChildObject != null)
				return new SingletonTargetMetadata(LoadTargetMetadata(tempChildObject, serializer), true);
			else if ((tempChildObject = jObject["$singleton"] as JObject) != null)
				return new SingletonTargetMetadata(LoadTargetMetadata(tempChildObject, serializer), false);

			//see if there's a 'construct' property.  If so, then we have a constructortarget call
			var tempTarget = jObject["$construct"];

			ITypeReference[] targetType;

			if (tempTarget != null)
			{
				if (tempTarget is JArray)
					targetType = tempTarget.ToObject<TypeReference[]>(serializer);
				else if (tempTarget is JObject)
					targetType = new[] { tempTarget.ToObject<TypeReference>(serializer) };
				else if (tempTarget is JValue)
				{
					string typeString = (string)tempTarget;
					if (string.IsNullOrWhiteSpace(typeString))
						throw new JsonConfigurationException("Target, if a string, must not be null, empty or whitespace", tempTarget);

					//if ("$self".Equals(typeString, StringComparison.OrdinalIgnoreCase))
					//	targetType = regTypes;	//we will allow multiple types to be specified for constructor target.  Thinking 
					////that so long as there's only one concrete class in the list, then we can be 'clever'
					//else
						targetType = new[] { new TypeReference(typeString) };	//otherwise deserialise the type reference
				}
				else
					throw new JsonConfigurationException("Unable to determine target type for Constructor Target metadata", jObject);

				return new ConstructorTargetMetadata(targetType);
			}

			//now see if there's a 'multi' property.  If so, then we have a an instruction to register multiple
			//targets against one set of type registrations.
			tempTarget = jObject["$multi"];

			if(tempTarget != null)
			{
				throw new NotImplementedException("Multi registration is not yet supported.");
			}

			//otherwise, we return an object target that will construct an instance of the requested type
			//from the raw Json.  This allows developers to implement Json Conversion for types specifically
			//for the purposes of reading from rezolver configuration 
			return CreateDeferredJsonDeserializerTarget(jObject, serializer);

			//throw new JsonConfigurationException("Unsupported target", jObject);
		}

		private static IRezolveTargetMetadata CreateDeferredJsonDeserializerTarget(JToken jToken, JsonSerializer serializer)
		{
			return new LazyJsonObjectTargetMetadata(jToken, serializer);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
