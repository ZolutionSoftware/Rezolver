// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using Rezolver.Targets;
using System;
using System.Linq.Expressions;

namespace Rezolver
{
	
	/// <summary>
	/// Contains numerous generic overloads of the RegisterExpression extension method for <see cref="ITargetContainer" />
	/// All of these extensions act as proxies for the 
	/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/> extension
	/// method. 
	/// </summary>
	public static partial class ExpressionTargetContainerExtensions
	{ 
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<TResult>(this ITargetContainer targetContainer, Expression<Func<TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T, TResult>(this ITargetContainer targetContainer, Expression<Func<T, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T14">The type of the 14th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T14">The type of the 14th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T14">The type of the 14th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T15">The type of the 15th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this ITargetContainer targetContainer, Expression<Func<ResolveContext, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
		
		/// <summary>Creates a target from the lambda expression and registers it in the 
		/// <paramref name="targetContainer" />, optionally against the given <paramref name="type" />.</summary>
		/// <typeparam name="T1">The type of the 1st lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T2">The type of the 2nd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T3">The type of the 3rd lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T4">The type of the 4th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T5">The type of the 5th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T6">The type of the 6th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T7">The type of the 7th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T8">The type of the 8th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T9">The type of the 9th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T10">The type of the 10th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T11">The type of the 11th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T12">The type of the 12th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T13">The type of the 13th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T14">The type of the 14th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="T15">The type of the 15th lambda parameter (will be resolved automatically)</typeparam>
		/// <typeparam name="TResult">The type of object produced by the lambda expression.</typeparam>
		/// <param name="targetContainer">The container which will receive the registration</param>
		/// <param name="expression">The lambda expression that is to be converted into a target and registered</param>
		/// <param name="type">The type for which the registration will be made, if different from the expression's type</param>
		/// <remarks>This function ultimately forwards the call through to the non generic extension method
		/// <see cref="ExpressionTargetContainerExtensions.RegisterExpression(ITargetContainer, Expression, Type)"/>, which 
		/// simply wraps the expression into an <see cref="ExpressionTarget"/> and then registers it against either 
		/// the passed <paramref name="type" />, or the created target's <see cref="ITarget.DeclaredType" /> if not provided.
		/// </remarks>
		public static void RegisterExpression<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>(this ITargetContainer targetContainer, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>> expression, Type type = null)
		{
			targetContainer.RegisterExpression((Expression)expression, type);
		}
	}
}
