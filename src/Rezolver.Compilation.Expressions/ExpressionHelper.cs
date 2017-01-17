using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    internal static class ExpressionHelper
    {
		/// <summary>
		/// A MethodInfo object representing the <see cref="LifetimeScopeRezolverExtensions.GetScopeRoot(IScopedContainer)"/> method - to aid in code generation
		/// where the target scope for tracking an object is the root scope, not the current scope.
		/// </summary>
		public static readonly MethodInfo Scope_GetScopeRootMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetScopeRoot(null));

		/// <summary>
		/// A MethodInfo object representing the generic definition <see cref="LifetimeScopeRezolverExtensions.GetOrAdd{T}(IScopedContainer, ResolveContext, Func{ResolveContext, T}, bool, bool)"/>
		/// </summary>
		public static readonly MethodInfo Scope_GetOrAddGenericMethod = MethodCallExtractor.ExtractCalledMethod(() => LifetimeScopeRezolverExtensions.GetOrAdd<object>(null, null, null, false)).GetGenericMethodDefinition();

		/// <summary>
		/// Returns an expression that represents a call to the <see cref="Scope_GetScopeRootMethod"/> extension method on the scope of the 
		/// ResolveContext passed to a compiled object target.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public static Expression Make_Scope_GetScopeRootCallExpression(IExpressionCompileContext context)
		{
			return Expression.Call(Scope_GetScopeRootMethod, context.ContextScopePropertyExpression);
		}

		/// <summary>
		/// Makes an expression which represents calling the <see cref="LifetimeScopeRezolverExtensions.GetOrAdd{T}(IScopedContainer, ResolveContext, Func{ResolveContext, T}, bool)"/>
		/// function for the passed <paramref name="objectType"/>.
		/// 
		/// Used automatically by the built-in scope tracking behaviour, but can also be used by your own custom target if you want
		/// to take control of its scope tracking behaviour.
		/// </summary>
		/// <param name="context">The compile context.</param>
		/// <param name="objectType">Type of the object to be stored or retrieved.</param>
		/// <param name="factoryExpression">Lambda which should be executed to obtain a new instance if a matching object is not already in scope.</param>
		/// <param name="iDisposableOnly">Expected to be a boolean expression indicating whether only IDisposables should be tracked in the scope.  The default 
		/// (if not provided) then will be set to 'true'.</param>
		public static Expression Make_Scope_GetOrAddCallExpression(IExpressionCompileContext context, Type objectType, LambdaExpression factoryExpression, Expression iDisposableOnly = null)
		{
			return Expression.Call(Scope_GetOrAddGenericMethod.MakeGenericMethod(objectType),
				context.ContextScopePropertyExpression,
				context.ResolveContextExpression,
				factoryExpression,
				iDisposableOnly ?? Expression.Constant(true));
		}
	}
}
