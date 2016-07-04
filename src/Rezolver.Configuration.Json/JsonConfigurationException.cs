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
	/// Thrown when a Json rezolver configuration file is invalid.
	/// </summary>
	public class JsonConfigurationException : Exception
	{
		/// <summary>
		/// If not null, then this is a reader whose position should be at the place where the error occurs.
		/// </summary>
		public JsonReader Reader { get; private set; }

		/// <summary>
		/// Constructs a new instance of the JsonConfigurationException class.
		/// 
		/// Note, if you supply a JsonReader, then the current line and column 
		/// will be reported automatically at the end of the exception message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="reader"></param>
		public JsonConfigurationException(string message, JsonReader reader)
			: base(message ?? "The JSON configuration is invalid") 
		{
			Reader = reader;
		}

		/// <summary>
		/// Constructs a new instance of the  JsonConfigurationException class.
		/// 
		/// Note, if you supply a JObject, then the starting line and column of that
		/// object from the original source text will be reported automatically at the
		/// end of the exception message.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="jToken"></param>
		public JsonConfigurationException(string message, JToken jToken)
			: this(message, jToken != null ? jToken.CreateReader() : null)
		{

		}

		/// <summary>
		/// Constructs a new instance of the JsonConfigurationException class which reports
		/// that the token at the current location of the file was not expected.
		/// </summary>
		/// <param name="expectedTokenType"></param>
		/// <param name="reader"></param>
		public JsonConfigurationException(JsonToken expectedTokenType, JsonReader reader)
			: this(string.Format("Unexpected token type '{0}', expected '{1}'", reader != null ? reader.TokenType.ToString() : "[null]", expectedTokenType), reader)
		{

		}

		public override string Message
		{
			get
			{
				return Reader != null ? ((IJsonLineInfo)Reader).FormatMessageForThisLine(base.Message) : base.Message;
			}
		}
	}
}
