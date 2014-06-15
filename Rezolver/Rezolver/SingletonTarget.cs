using System;
using System.Linq.Expressions;
using System.Threading;

namespace Rezolver
{
	/// <summary>
	/// Decorates any IRezolveTarget instance inside a lazily-initialised instance which will only ever
	/// create the target object once.
	/// 
	/// TODO: I don't actually think this will work properly because the scope is not responsible for actually
	/// creating the instances - a Rezolver will be.  This target is more an instruction on how to build a given instance.
	/// </summary>
	public class SingletonTarget : IRezolveTarget
	{
		private IRezolveTarget _innerTarget;
		private Func<IRezolverScope, Type, Expression> _createExpressionDelegate;  
		//this is the most naive implementation of this class - bake a lazy into this singleton,
		//initialise it with a delegate built from the inner target's expression, and then this
		//one's expression simply returns this lazy's value.
		//note that this lazy is not actually initialised if the inner target is another SingletonTarget
		private Lazy<object> _lazy; 

		public SingletonTarget(IRezolveTarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			this._innerTarget = innerTarget;
			//if the passed target is another Singleton target, then import its lazy target reference
			//to this one rather than wrapping it in another one.
			SingletonTarget singletonTarget = innerTarget as SingletonTarget;

			if (singletonTarget != null)
				_createExpressionDelegate = CreateExpressionFromInnerSingleton;
			else
				_createExpressionDelegate = CreateExpressionFromInner;
			//can't create the lazy until we get a scope on the first CreateExpression call
		}

		public bool SupportsType(Type type)
		{
			return _innerTarget.SupportsType(type);
		}

		private Expression CreateExpressionFromInnerSingleton(IRezolverScope scope, Type targetType)
		{
			return ((SingletonTarget) _innerTarget).CreateExpression(scope, targetType);
		}

		private Expression CreateExpressionFromInner(IRezolverScope scope, Type targetType)
		{
			if(_lazy == null)
				_lazy = new Lazy<object>(Expression.Lambda<Func<object>>(_innerTarget.CreateExpression(scope, typeof(object))).Compile());
			Expression<Func<object>> e = () => _lazy.Value;
			return Expression.Convert(e.Body, targetType ?? DeclaredType);
		}

		public Expression CreateExpression(IRezolverScope scope, Type targetType = null)
		{
			return _createExpressionDelegate(scope, targetType);
		}

		public Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}
	}
}
