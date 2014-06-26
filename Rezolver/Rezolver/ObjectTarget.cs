﻿using System;
﻿using System.Collections.Generic;
﻿using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Implements IRezolveTarget by wrapping a single instance.
	/// 
	/// Note that, while that effectively makes this target a singleton, there is a special type for a singleton target
	/// which is used to decorate any other target.
	/// </summary>
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
					throw new ArgumentException(string.Format(Resources.Exceptions.DeclaredTypeIsNotCompatible_Format, declaredType, _object.GetType()), "declaredType");

				_declaredType = declaredType;
			}
			else //an untyped null is typed as Object
				_declaredType = _object == null ? typeof(object) : _object.GetType();	
		}

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null, Stack<IRezolveTarget> currentTargets = null)
		{
			return Expression.Constant(_object, targetType ?? DeclaredType);
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
