using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	/// <summary>
	/// Special version of IObjectTargetMetadata which creates an instance of the 
	/// requested type from a JToken.  Some more work might be required here
	/// to 
	/// </summary>
	public class LazyJsonObjectTargetMetadata : IObjectTargetMetadata
	{
		private readonly object _locker = new object();
		private object _object;
		private readonly JToken _token;
		private readonly JsonSerializer _serializer;

		internal LazyJsonObjectTargetMetadata(JToken token, JsonSerializer serializer)
		{
			_token = token;
			_serializer = serializer;
		}

		public object GetObject(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			if(_object == null)
			{
				lock(_locker)
				{
					if(_object == null)
					{
						try
						{
							_object = _token.ToObject(type, _serializer);
						}
						catch(Exception ex)
						{
							throw new ArgumentException(string.Format("Unable to deserialize object of type {0} from the underlying JSON data", type), ex);
						}
					}
				}
			}
			return _object;
		}

		public RezolveTargetMetadataType Type
		{
			get { return RezolveTargetMetadataType.Object; }
		}
	}
}
