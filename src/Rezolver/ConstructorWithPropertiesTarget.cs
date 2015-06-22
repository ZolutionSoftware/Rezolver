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
                //have to locate the NewExpression constructed by the inner target and then rewrite it as
                //a MemberInitExpression with the given property bindings.  Note - if the expression created
                //by the ConstructorTarget is surrounded with any non-standard funny stuff - i.e. anything that
                //could require a NewExpression, then this code won't work.  Points to the possibility that we
                //might need some additional funkiness to allow code such as this to do its thing.
                return new NewExpressionMemberInitRewriter(null,
                    _propertyBindings.Select(b => b.CreateMemberBinding(new CompileContext(context, b.MemberType, true)))).Visit(nestedExpression);
			}
		}

		public override Type DeclaredType
		{
			get { return _nestedTarget.DeclaredType; }
		}
	}
}
