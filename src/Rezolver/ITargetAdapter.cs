// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Interface for an object that can convert Expressions into <see cref="ITarget"/>s suitable for
	/// registration in an <see cref="ITargetContainer"/>
	/// </summary>
	/// <remarks>If you are considering manually creating an <see cref="ExpressionTarget"/> for an expression, then
	/// you should consider instead using an implementation of this interface to produce an <see cref="ITarget"/>
	/// for that expression.
	/// 
	/// For example, the <see cref="TargetAdapter"/> class supplied by the framework, has the
	/// ability to convert some common code constructs into specific types of targets so as to properly leverage the power
	/// of the Rezolver framework at runtime.
	/// 
	/// That class also has the ability to translate a whole lambda expression into a target, including converting any parameters
	/// for that lambda into injected variables.</remarks>
	public interface ITargetAdapter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		ITarget CreateTarget(Expression expression);
	}

	public static class TargetAdapterCreateExtensions
	{
		public static ITarget CreateTarget<TResult>(this ITargetAdapter adapter, Expression<Func<TResult>> expression)
		{
			adapter.MustNotBeNull(nameof(adapter));
			expression.MustNotBeNull(nameof(expression));

			return adapter.CreateTarget((Expression)expression);
		}

		public static ITarget CreateTarget<TResult>(this ITargetAdapter adapter, Expression<Func<RezolveContext, TResult>> expression)
		{
			adapter.MustNotBeNull("adapter");
			expression.MustNotBeNull("expression");

			return adapter.CreateTarget((Expression)expression);
		}
	}
}