using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// This static class contains methods and properties to aid in building expressions suitable to be used in
	/// rezolver targets
	/// </summary>
	public static class ExpressionHelper
	{
		/// <summary>
		/// This parameter expression is to be used by all targets and rezolvers in this library by default to perform late binding 
		/// to a rezolver provided at run time when a caller is trying to resolve something through code built from
		/// a target.
		/// </summary>
		public static readonly ParameterExpression DynamicRezolverParam = Expression.Parameter(typeof (IRezolver),
			"dynamicRezolver");

		public static Expression<Func<IRezolver, object>> GetLambdaForTarget(IRezolver rezolver, Type type, IRezolveTarget target, Stack<IRezolveTarget> currentTargets = null)
		{
			return Expression.Lambda<Func<IRezolver, object>>(
				Expression.Convert(target.CreateExpression(rezolver, targetType: type, currentTargets: currentTargets), typeof(object)),
				DynamicRezolverParam);
		}

		public static Func<IRezolver, object> GetFactoryForTarget(IRezolver rezolver, Type type,
			IRezolveTarget target, Stack<IRezolveTarget> currentTargets = null)
		{
#if DEBUG
			var lambda = GetLambdaForTarget(rezolver, type, target, currentTargets);
			Debug.WriteLine("ExpressionHelper is Compiling lambda \"{0}\" for type {1}", lambda, type);
			return lambda.Compile();
#else
			return GetLambdaForTarget(rezolver, type, target).Compile();
#endif

		}

	}
}