﻿using System;
using System.Collections.Generic;
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
	public class SingletonTarget : RezolveTargetBase
	{
		private IRezolveTarget _innerTarget;
		private Func<IRezolverContainer, Type, ParameterExpression, Stack<IRezolveTarget>, Expression> _createExpressionDelegate;  
		//this is the most naive implementation of this class - bake a lazy into this singleton,
		//initialise it with a delegate built from the inner target's expression, and then this
		//one's expression simply returns this lazy's value.
		//note that this lazy is not actually initialised if the inner target is another SingletonTarget
		private Lazy<object> _lazy; 

		public SingletonTarget(IRezolveTarget innerTarget)
		{
			innerTarget.MustNotBeNull("innerTarget");
			_innerTarget = innerTarget;
			//if the passed target is another Singleton target, then import its lazy target reference
			//to this one rather than wrapping it in another one.
			SingletonTarget singletonTarget = innerTarget as SingletonTarget;

			if (singletonTarget != null)
				_createExpressionDelegate = CreateExpressionFromInnerSingleton;
			else
				_createExpressionDelegate = CreateExpressionFromInner;
			//can't create the lazy until we get a scope on the first CreateExpression call
		}

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null, ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			return _createExpressionDelegate(scopeContainer, targetType, dynamicContainerExpression, currentTargets);
		}

		private Expression CreateExpressionFromInnerSingleton(IRezolverContainer containerScope, Type targetType, ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			return ((SingletonTarget) _innerTarget).CreateExpression(containerScope, targetType: targetType, currentTargets: currentTargets);
		}

		private Expression CreateExpressionFromInner(IRezolverContainer scope, Type targetType, ParameterExpression dynamicContainerExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			//BUG: This will not honour dynamic containers being passed at runtime as it's not being forwarded to the lazy.
			//The only solution to this will be to create a class dynamically which creates the lazy in the dynamic code.  I think.
			if (_lazy == null)
			{
				var target = scope.Compiler.CompileTarget(_innerTarget, scope, dynamicContainerExpression, currentTargets);
				_lazy = new Lazy<object>(target.GetObject);
			}

			return Expression.Property(Expression.Constant(_lazy), "Value");
		}

		public override Type DeclaredType
		{
			get { return _innerTarget.DeclaredType; }
		}
	}
}
