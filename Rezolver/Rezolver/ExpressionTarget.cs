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
		private readonly Type _declaredType;//used only when a factory is used.
		private readonly Func<CompileContext, Expression> _expressionFactory;

		public ExpressionTarget(Expression expression, IRezolveTargetAdapter adapter = null)
		{
			//TODO: null check
			_expression = expression;
		}

		public ExpressionTarget(Func<CompileContext, Expression> expressionFactory, Type declaredType, IRezolveTargetAdapter adapter = null)
		{
			_expressionFactory = expressionFactory;
			_declaredType = declaredType;
		}

		protected override Expression CreateExpressionBase(CompileContext context)
		{
			return _expression ?? _expressionFactory(context);
		}

		public override Type DeclaredType
		{
			get { return _expressionFactory != null ? _declaredType : _expression.Type; }
		}
	}
}