using System;
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

		protected override Expression CreateExpressionBase(IRezolverScope scope, Type targetType = null)
		{
			return _expression;
		}

		public override Type DeclaredType
		{
			get { return _expression.Type; }
		}
	}
}