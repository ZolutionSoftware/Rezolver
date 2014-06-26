using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// This static class contains methods and properties to aid in building expressions suitable to be used in
	/// building IRezolverContainer factory delegates from IRezolveTarget.
	/// </summary>
	public static class ExpressionHelper
	{
		/// <summary>
		/// This parameter expression is to be used by all targets and containers in this library to perform late binding 
		/// to a container provided at run time when a caller is trying to resolve something through a delegate built from
		/// a target.
		/// </summary>
		public static readonly ParameterExpression DynamicContainerParam = Expression.Parameter(typeof (IRezolverContainer),
			"containerScope");

		public static readonly MethodInfo RezolveContainerRezolveMethod = MethodCallExtractor.ExtractCalledMethod(
			(IRezolverContainer container) => container.Rezolve(null, null, null);

		/// <summary>
		/// Returns a MethodCallExpression invoked on the <see cref="DynamicContainerParam"/> (which is used by all 
		/// IRezolveTarget and IRezolverContainer implementations provided in this library) to represent a dynamic 
		/// container being passed to a delegate built from expressions produced by an IRezolveTarget.
		/// 
		/// The returned expression will include any necessary type conversion required to convert from object
		/// up to the target type, since the IRezolverContainer's Rezolve operation is not generic.
		/// </summary>
		/// <param name="targetType">An expression representing the target type that is to be passed to the </param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Expression GetDynamicRezolveCall(Type targetType, Expression name)
		{
			//notice here that the third parameter of the IRezolveContainer's Rezolve method - i.e. another scope - is not
			//required for this expression, because it should only be used on a dynamic container being passed into a
			//Rezolve call, and it would make no sense to forward that container on to itself.
			return
				Expression.Convert(
					Expression.Call(
						DynamicContainerParam, 
						RezolveContainerRezolveMethod, 
						Expression.Constant(targetType), 
						name,
						Expression.Constant(null, typeof (IRezolverContainer))), targetType);
		}

		public static Expression<Func<IRezolverContainer, object>> GetLambdaForTarget(IRezolverContainer scopeContainer, Type type, IRezolveTarget target)
		{
			return Expression.Lambda<Func<IRezolverContainer, object>>(
				Expression.Convert(target.CreateExpression(scopeContainer, targetType: type), typeof(object)),
				DynamicContainerParam);
		}

		public static Func<IRezolverContainer, object> GetFactoryForTarget(IRezolverContainer scopeContainer, Type type,
			IRezolveTarget target)
		{
			return GetLambdaForTarget(scopeContainer, type, target).Compile();
		}
	}
}