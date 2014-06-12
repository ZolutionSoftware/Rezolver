﻿using System;

namespace Rezolver
{
	public class ObjectTarget : RezolveTargetBase
	{
		private readonly object _object;
		private readonly Type _declaredType;

		public ObjectTarget(object obj, Type declaredType = null)
		{
			_object = obj;
			//if the caller provides a declared type we check
			//also that, if the object is null, the target type
			//can accept nulls.  Otherwise we're simply checking 
			//that the value that's supplied is compatible with the 
			//type that is being declared.
			if (declaredType != null)
			{
				if (_object == null)
				{
					if (!TypeHelpers.CanBeNull(declaredType))
						throw new ArgumentException(string.Format(Resources.Exceptions.TargetIsNullButTypeIsNotNullable_Format, declaredType), "declaredType");
				}
				else if (!TypeHelpers.AreCompatible(_object.GetType(), declaredType))
					throw new ArgumentException(string.Format("", _object.GetType(), declaredType), "declaredType");

				_declaredType = declaredType;
			}
			else //an untyped null is typed as Object
				_declaredType = _object == null ? typeof(object) : _object.GetType();	
		}

		public override object GetObject()
		{
			return _object;
		}

		public override Type DeclaredType
		{
			get
			{
				return _declaredType;
			}
		}
	}

	public static class ObjectTargetExtensions
	{
		public static ObjectTarget AsObjectTarget<T>(this T obj, Type declaredType = null)
		{
			return new ObjectTarget(obj, declaredType);
		}
	}
}
