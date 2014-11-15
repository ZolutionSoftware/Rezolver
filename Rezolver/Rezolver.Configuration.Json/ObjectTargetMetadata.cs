using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration.Json
{
	internal class ObjectTargetMetadata : IRezolveTargetMetadata
	{
		private readonly Func<object> _valueProvider;
		private readonly Type _type;
		public ObjectTargetMetadata(object obj, Type type = null)
		{
			_valueProvider = () => obj;
			_type = type;
		}

		public RezolveTargetMetadataType Type
		{
			get { return RezolveTargetMetadataType.Object; }
		}
	}

}
