// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
      [JsonProperty("name")]
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

      [JsonProperty("array")]
      public bool IsArray
      {
        get;
        set;
      }

      [JsonIgnore]
      public bool IsUnbound
      {
        get
        {
          return JsonConfiguration.UnboundType == TypeName;
        }
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
      IJsonLineInfo lineInfo = (reader as IJsonLineInfo) ?? StubJsonLineInfo.Instance;

      var startLine = lineInfo.Capture();

      if (reader.TokenType == JsonToken.String)
      {
        //note - line information for this will have the same start and end line/pos
        return new TypeReference(reader.Value as string, startLine.ToConfigurationLineInfo(lineInfo), false, JsonConfiguration.UnboundType.Equals(reader.Value));
      }
      else if (reader.TokenType == JsonToken.StartObject)
      {
        var temp = new JsonTypeReference();
        serializer.Populate(reader, temp);
        return new TypeReference(temp.TypeName, startLine.ToConfigurationLineInfo(lineInfo), temp.IsArray, temp.IsUnbound, temp.GenericArguments);
      }
      else throw new InvalidOperationException(((reader as IJsonLineInfo) ?? StubJsonLineInfo.Instance).FormatMessageForThisLine("Invalid Type Reference"));
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotImplementedException();
    }
  }
}
