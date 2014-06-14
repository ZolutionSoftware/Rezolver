using System;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// As the name suggests, the underlying target of a Rezolver call.  The output of a 
	/// rezolve target is an expression.  This allows a Rezolve target that depends on another
	/// rezolve target to chain expressions together, creating specialised expression trees (and
	/// therefore specialised delegates).
	/// </summary>
	public interface IRezolveTarget
	{
		bool SupportsType(Type type);
		Expression CreateExpression(IRezolverScope scope, Type targetType = null);
		Type DeclaredType { get; }
	}
}
