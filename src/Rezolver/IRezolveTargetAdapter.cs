using System;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that is responsible for converting Expressions into IRezolveTargets.
	/// </summary>
	public interface IRezolveTargetAdapter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		IRezolveTarget GetRezolveTarget(Expression expression);
	}

	public static class RezolveTargetAdapterExtensions
	{
		public static IRezolveTarget GetRezolveTarget<T>(this IRezolveTargetAdapter adapter, Expression<Func<T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");
// ReSharper disable RedundantCast
			return adapter.GetRezolveTarget((Expression) expression);
// ReSharper restore RedundantCast
		}

		public static IRezolveTarget GetRezolveTarget<T>(this IRezolveTargetAdapter adapter,
			Expression<Func<RezolveContextExpressionHelper, T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");
// ReSharper disable RedundantCast
			return adapter.GetRezolveTarget((Expression) expression);
// ReSharper restore RedundantCast
		}
	}
}