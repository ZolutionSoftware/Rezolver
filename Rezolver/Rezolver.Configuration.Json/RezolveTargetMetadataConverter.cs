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
						JValue value = JValue.ReadFrom(reader) as JValue;
						//have to create a lazy target here - because otherwise value literals will be 
						//added to the rezolverbuilder using the type that Json.Net gives them, but that's not
						//always correct.  Integers read using the JsonReader, for example, are read as Int64,
						//which is no good when you want to read the value as Int32 - you can't cast, you have
						//to convert.  Which is no good for us.  Using a lazy means we can use Json.Net's own
						//handling for this - e.g. taking an integer token and reading it as an Int16 or Int32 
						//will 'just work'.
						meta = new LazyJsonObjectTargetMetadata(value, serializer);
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

			//see if there's a 'construct' property.  If so, then we have a constructor target call
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
						throw new JsonConfigurationException("Target, if a string, must not be null, empty or white space", tempTarget);

					targetType = new[] { new TypeReference(typeString, ((IJsonLineInfo)tempTarget).ToConfigurationLineInfo() )};	//otherwise deserialise the type reference
				}
				else
					throw new JsonConfigurationException("Unable to determine target type for Constructor Target metadata", jObject);
				//TODO: add specific constructor argument matching (but if you specify one you have to specify all)
				//and the ability to set properties.  Although, the current constructor target doesn't yet support that either.
				return new ConstructorTargetMetadata(targetType);
			}

			//now see if there's a '$targets' property.  if so, it's a special case target metadata object which instructs the parser
			//to read an array of nested metadata objects.  If this is specified as an object to be registered as a TypeConfigurationEntry,
			//then it will cause the list of targets to be grouped together as a multiple registration.
			tempTarget = jObject["$targets"];

			if(tempTarget != null)
				return CreateTargetMetadataList(tempTarget, serializer);

			bool isArray = false;
			tempTarget = jObject["$array"];
			if (tempTarget != null)
				isArray = true;
			else
				tempTarget = jObject["$list"];

			if (tempTarget != null)
				return CreateListTargetMetadata(jObject, tempTarget, isArray, serializer);


			//otherwise, we return an object target that will construct an instance of the requested type
			//from the raw Json.  This allows developers to implement Json Conversion for types specifically
			//for the purposes of reading from rezolver configuration - note, however, that when doing this it's
			//not possible to 
			return CreateDeferredJsonDeserializerTarget(jObject, serializer);

			//throw new JsonConfigurationException("Unsupported target", jObject);
		}

		private IRezolveTargetMetadata CreateListTargetMetadata(JToken jObject, JToken elementTypeToken, bool isArray, JsonSerializer serializer)
		{
			ITypeReference elementType = elementTypeToken.ToObject<TypeReference>();
			var values = jObject["values"] as JArray;
			if (values == null)
				throw new JsonConfigurationException("Expected array property 'values' for List/Array target metadata", jObject);
			var valuesMetadataList = CreateTargetMetadataList(values, serializer);
			return new ListTargetMetadata(elementType, valuesMetadataList, isArray);
		}

		private IRezolveTargetMetadataList CreateTargetMetadataList(JToken tempTarget, JsonSerializer serializer)
		{
			if (tempTarget is JArray)
			{
				var targets = tempTarget.Select(t => t.ToObject<RezolveTargetMetadataWrapper>(serializer));
				return new RezolveTargetMetadataList(targets);
			}
			else
				throw new JsonConfigurationException("Expected an array of target metadata entries", tempTarget);
		}

		private static IRezolveTargetMetadata	CreateDeferredJsonDeserializerTarget(JToken jToken, JsonSerializer serializer)
		{
			return new LazyJsonObjectTargetMetadata(jToken, serializer);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}
	}
}
