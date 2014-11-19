using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ObjectTargetMetadata : RezolveTargetMetadataBase
	{
		private readonly Func<object> _valueProvider;
		private readonly Type _type;
		public ObjectTargetMetadata(object obj, Type type = null)
			: base(RezolveTargetMetadataType.Object)
		{
			_valueProvider = () => obj;
			_type = type;
		}

		public ObjectTargetMetadata(Func<object> valueProvider, Type type = null)
			: base(RezolveTargetMetadataType.Object)
		{
			_valueProvider = valueProvider;
			_type = type;
		}

		public RezolveTargetMetadataType Type
		{
			get { return RezolveTargetMetadataType.Object; }
		}
	}

}
