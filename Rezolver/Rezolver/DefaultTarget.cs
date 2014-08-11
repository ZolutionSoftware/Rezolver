using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	public class DefaultTarget : RezolveTargetBase
	{
		private readonly Type _declaredType;

		public DefaultTarget(Type type)
		{
			type.MustNotBeNull("type");
			_declaredType = type;
		}

		protected override Expression CreateExpressionBase(IRezolver rezolver, Type targetType = null, ParameterExpression dynamicRezolverExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			return Expression.Default(DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}
}