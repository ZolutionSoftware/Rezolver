﻿using System;
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
			if(_object != null)
				return type.IsAssignableFrom(_object.GetType());

			if(type.IsValueType)
			{
				if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
					return true;
				return false;
			}
			return true;
		}
	}
}
