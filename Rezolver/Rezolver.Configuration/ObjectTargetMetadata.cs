using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public class ObjectTargetMetadata : RezolveTargetMetadataBase, IObjectTargetMetadata
	{
		private readonly Func<Type, object> _valueProvider;
		private readonly Type _type;

		public RezolveTargetMetadataType Type
		{
			get { return RezolveTargetMetadataType.Object; }
		}

		public ObjectTargetMetadata(object obj, Type type = null)
			: base(RezolveTargetMetadataType.Object)
		{
			_valueProvider = (t) => obj;
			_type = type;
		}

		public ObjectTargetMetadata(Func<Type, object> valueProvider, Type type = null)
			: base(RezolveTargetMetadataType.Object)
		{
			_valueProvider = valueProvider;
			_type = type;
		}

		public virtual object GetObject(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");

			return _valueProvider(type);
		}
	}

}
