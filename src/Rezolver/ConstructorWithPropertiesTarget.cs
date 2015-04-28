using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Wraps a constructor target so that properties can be bound also.
	/// </summary>
	public class ConstructorWithPropertiesTarget : RezolveTargetBase
	{
		PropertyOrFieldBinding[] _propertyBindings;
		ConstructorTarget _nestedTarget;

		/// <summary>
		/// Please note that with this constructor, no checking is performed that the property bindings are
		/// actually valid for the target's type.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="propertyBindings"></param>
		public ConstructorWithPropertiesTarget(ConstructorTarget target, PropertyOrFieldBinding[] propertyBindings)
			: base()
		{
			_nestedTarget = target;
			_propertyBindings = propertyBindings ?? PropertyOrFieldBinding.None;
		}

		//TODO: add a constructor that takes an IPropertyBindingBehaviour instance so that
		//propery bindings can be resolved either in the constructor or even later.

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			if (_propertyBindings.Length == 0)
				return _nestedTarget.CreateExpression(context);
			else
			{
				var nestedExpression = _nestedTarget.CreateExpression(new CompileContext(context, _nestedTarget.DeclaredType, true));
				var newExpression = nestedExpression as NewExpression;

				if (newExpression == null)
					throw new InvalidOperationException(string.Format("Nested ConstructorTarget must create a NewExpression to be used with this type, but created an instance of {0}", nestedExpression.GetType()));

				return Expression.MemberInit((NewExpression)newExpression,
					_propertyBindings.Select(b => b.CreateMemberBinding(new CompileContext(context, b.MemberType, true))));
			}
		}

		public override Type DeclaredType
		{
			get { return _nestedTarget.DeclaredType; }
		}
	}
}
