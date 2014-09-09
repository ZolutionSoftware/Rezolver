using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// A ConstructorTarget 
	/// </summary>
	public class ConstructorWithPropertiesTarget : ConstructorTarget
	{
		PropertyOrFieldBinding[] _propertyBindings;

		public ConstructorWithPropertiesTarget(Type declaredType, ConstructorInfo ctor, ParameterBinding[] parameterBindings, PropertyOrFieldBinding[] propertyBindings)
			: base(declaredType, ctor, parameterBindings)
		{
			_propertyBindings = propertyBindings ?? PropertyOrFieldBinding.None;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			if (_propertyBindings.Length == 0)
				return base.CreateExpressionBase(context);
			else
				return Expression.MemberInit((NewExpression)base.CreateExpressionBase(context),
					_propertyBindings.Select(b => b.CreateMemberBinding(new CompileContext(context, b.MemberType, true))));
		}
	}
}
