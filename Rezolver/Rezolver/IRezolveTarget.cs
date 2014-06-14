using System;
using System.Linq.Expressions;

namespace Rezolver
{
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);
		Expression CreateExpression(IRezolverScope scope, Type targetType = null);
		Type DeclaredType { get; }
	}
}
