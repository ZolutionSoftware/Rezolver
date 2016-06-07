using System;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that is responsible for converting Expressions into <see cref="ITarget"/>s.
	/// </summary>
	public interface ITargetAdapter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		ITarget CreateTarget(Expression expression);
	}

	public static class RezolveTargetAdapterExtensions
	{
		public static ITarget CreateTarget<T>(this ITargetAdapter adapter, Expression<Func<T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");

			return adapter.CreateTarget((Expression)expression);
		}

		public static ITarget CreateTarget<T>(this ITargetAdapter adapter,
			Expression<Func<RezolveContextExpressionHelper, T>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");

			return adapter.CreateTarget((Expression) expression);
		}
	}
}