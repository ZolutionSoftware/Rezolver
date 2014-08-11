using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// A generic target for all expressions not explicitly supported by a particular target.
	/// </summary>
	public class ExpressionTarget : RezolveTargetBase
	{
		private readonly Expression _expression;

		public ExpressionTarget(Expression expression)
		{
			//TODO: null check
			_expression = expression;
		}

		protected override Expression CreateExpressionBase(IRezolver rezolver, Type targetType = null, ParameterExpression dynamicRezolverExpression = null, Stack<IRezolveTarget> currentTargets = null)
		{
			return _expression;
		}

		public override Type DeclaredType
		{
			get { return _expression.Type; }
		}
	}
}