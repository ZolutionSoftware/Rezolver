using System;
using System.Linq.Expressions;

namespace Rezolver
{
	public abstract class RezolveTargetBase : IRezolveTarget
	{

		public virtual bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			return TypeHelpers.AreCompatible(DeclaredType, type);
		}

		public abstract Expression CreateExpression(IRezolverScope scope, Type targetType = null);

		public abstract Type DeclaredType
		{
			get;
		}
	}
}
