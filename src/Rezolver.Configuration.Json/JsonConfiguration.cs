// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
  /// <summary>
  /// Class JsonConfiguration.
  /// </summary>
  [JsonConverter(typeof(JsonConfigurationConverter))]
  public class JsonConfiguration : IConfiguration
  {
    /// <summary>
    /// The typename to use on a type entry when you want to register a constructor target directly for the type that is registered.
    /// E.g. { 'MyNamespace.Foo' : { 'type' : "$auto' } }
    /// Which registers MyNamespace.Foo with a constructor target that binds to the same type.
    /// 
    /// Is also used - where supported - to refer to a type from a parent object which you want to reference, but without having to
    /// specify the whole typename.
    /// </summary>
    public const string UnboundType = "$auto";

    /// <summary>
    /// Gets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the entries.
    /// </summary>
    /// <value>The entries.</value>
    public IEnumerable<IConfigurationEntry> Entries
    {
      get;
      private set;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonConfiguration"/> class.
    /// </summary>
    /// <param name="entries">The entries.</param>
    private JsonConfiguration(IEnumerable<IConfigurationEntry> entries)
    {
      Entries = entries;
    }

    /// <summary>
    /// Class for loading JsonConfiguration from Json through Json.Net.
    /// </summary>
    public class JsonConfigurationConverter : JsonConverter
    {
      /// <summary>
      /// Class ConfigurationProxy.
      /// </summary>
      private class ConfigurationProxy
      {
        /// <summary>
        /// The empty entries
        /// </summary>
        private static readonly IConfigurationEntry[] EmptyEntries = new IConfigurationEntry[0];
        /// <summary>
        /// The empty references
        /// </summary>
        private static readonly IAssemblyReferenceEntry[] EmptyReferences = new IAssemblyReferenceEntry[0];
        /// <summary>
        /// The empty namespace imports
        /// </summary>
        private static readonly IConfigurationEntry[] EmptyNamespaceImports = new IConfigurationEntry[0];

        /// <summary>
        /// The _rezolve entries
        /// </summary>
        private IConfigurationEntry[] _rezolveEntries;
        /// <summary>
        /// Gets or sets the rezolve entries.
        /// </summary>
        /// <value>The rezolve entries.</value>
        [JsonProperty("rezolve")]
        public IConfigurationEntry[] RezolveEntries { get { return _rezolveEntries ?? EmptyEntries; } set { _rezolveEntries = value; } }

        /// <summary>
        /// The _assemblies
        /// </summary>
        private IAssemblyReferenceEntry[] _assemblies;
        /// <summary>
        /// Note this property has an explicit JsonConverter to tell the serializer to deserialize assembly references.
        /// </summary>
        /// <value>The assemblies.</value>
        [JsonProperty("assemblies")]
        public IAssemblyReferenceEntry[] Assemblies { get { return _assemblies ?? EmptyReferences; } set { _assemblies = value; } }

        /// <summary>
        /// The _namespace imports
        /// </summary>
        private IConfigurationEntry[] _namespaceImports;
        /// <summary>
        /// Gets or sets the namespace imports.
        /// </summary>
        /// <value>The namespace imports.</value>
        [JsonProperty("using")]
        public IConfigurationEntry[] NamespaceImports { get { return _namespaceImports ?? EmptyNamespaceImports; } set { _namespaceImports = value; } }
      }

      /// <summary>
      /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
      /// </summary>
      /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
      public override bool CanWrite
      {
        get
        {
          return false;
        }
      }
      /// <summary>
      /// Determines whether this instance can convert the specified object type.
      /// </summary>
      /// <param name="objectType">Type of the object.</param>
      /// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
      public override bool CanConvert(Type objectType)
      {
        return TypeHelpers.IsAssignableFrom(typeof(JsonConfiguration), objectType);
      }

      /// <summary>
      /// Reads the JSON representation of the object.
      /// </summary>
      /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
      /// <param name="objectType">Type of the object.</param>
      /// <param name="existingValue">The existing value of object being read.</param>
      /// <param name="serializer">The calling serializer.</param>
      /// <returns>The object value.</returns>
      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        var proxy = serializer.Deserialize<ConfigurationProxy>(reader);

        return new JsonConfiguration(proxy.RezolveEntries.Concat(proxy.Assemblies).Concat(proxy.NamespaceImports).ToArray());
      }

      /// <summary>
      /// Writes the JSON representation of the object.
      /// </summary>
      /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
      /// <param name="value">The value.</param>
      /// <param name="serializer">The calling serializer.</param>
      /// <exception cref="System.NotImplementedException"></exception>
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        throw new NotImplementedException();
      }
    }
  }
}
