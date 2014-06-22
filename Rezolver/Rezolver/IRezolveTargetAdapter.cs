using System;
using System.Linq.Expressions;

namespace Rezolver
{
	public interface IRezolveTargetAdapter
	{
		IRezolveTarget ConvertToTarget(Expression expression);
	}

	public static class RezolveTargetAdapterExtensions
	{
		public static IRezolveTarget ConvertToTarget<T>(this IRezolveTargetAdapter adapter, Expression<Func<T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");
// ReSharper disable RedundantCast
			return adapter.ConvertToTarget((Expression) expression);
// ReSharper restore RedundantCast
		}

		public static IRezolveTarget ConvertToTarget<T>(this IRezolveTargetAdapter adapter,
			Expression<Func<IRezolverScope, T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");
// ReSharper disable RedundantCast
			return adapter.ConvertToTarget((Expression) expression);
// ReSharper restore RedundantCast
		}
	}
}