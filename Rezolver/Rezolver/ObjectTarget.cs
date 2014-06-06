using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class ObjectTarget : IRezolveTarget
	{
		private readonly object _object;

		public ObjectTarget(object obj)
		{
			this._object = obj;
		}

		public object GetObject()
		{
			return _object;
		}

		public bool SupportsType(Type type)
		{
			if (_object == null)
			{
				if (type.IsValueType)
				{
					if (type.IsGenericType)
					{
						//if the generic type definition is Nullable<> then null is supported.
						if (typeof(Nullable<>).Equals(type.GetGenericTypeDefinition()))
							return true;
					}
					return false;
				}
				else
					return true;
			}
			else
				return type.IsAssignableFrom(_object.GetType());
		}
	}
}
