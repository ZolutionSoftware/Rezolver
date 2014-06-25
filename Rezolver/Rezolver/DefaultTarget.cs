using System;
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

		protected override Expression CreateExpressionBase(IRezolverContainer scopeContainer, Type targetType = null)
		{
			return targetType != null && targetType != DeclaredType
				? (Expression)Expression.Convert(Expression.Default(DeclaredType), targetType)
				: Expression.Default(DeclaredType);
		}

		public override Type DeclaredType
		{
			get { return _declaredType; }
		}
	}
}