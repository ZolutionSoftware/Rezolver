﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class ObjectTarget : IRezolveTarget
	{
		private readonly object _object;
		private readonly Type _declaredType;

		public ObjectTarget(object obj, Type declaredType = null)
		{
			_object = obj;
			if (declaredType != null)
			{
				if (_object == null)
				{
					if (!TypeHelpers.CanBeNull(declaredType))
						throw new ArgumentException(string.Format(Resources.Exceptions.TargetIsNullButTypeIsNotNullable_Format, declaredType), "declaredType");
				}
				else if (!TypeHelpers.AreCompatible(_object.GetType(), declaredType))
					throw new ArgumentException(string.Format("", _object.GetType(), declaredType), "declaredType");
			}
			else //an untyped null is typed as Object
				_declaredType = _object == null ? typeof(object) : _object.GetType();	
		}

		public object GetObject()
		{
			return _object;
		}

		public bool SupportsType(Type type)
		{
			return TypeHelpers.AreCompatible(_declaredType, type);
			//ToDo: consider type conversions using implicit (and explicit?) conversion operators
			//being searched here.  
			//return false;
		}

		public Type DeclaredType
		{
			get
			{
				return _declaredType;
			}
		}
	}
}
