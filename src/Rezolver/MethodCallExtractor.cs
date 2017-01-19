// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	/// <summary>
	/// Used to help grab method and constructor info from expressions (which is easier than writing long
	/// strings of reflection code).
	/// 
	/// For example: <code>MethodCallExtractor.ExtractCalledMethod(() => Console.WriteLine("foo"))</code>
	/// 
	/// Will return the MethodInfo for the <c>WriteLine</c> method of the <c>Console</c> class.
	/// </summary>
	public sealed class MethodCallExtractor : ExpressionVisitor
	{
		private MethodCallExpression _callExpr;
		private NewExpression _newExpr;

		internal MethodInfo CalledMethod
		{
			get
			{
				return _callExpr?.Method;
			}
		}

		internal ConstructorInfo CalledConstructor
		{
			get
			{
				return _newExpr?.Constructor;
			}
		}

		private MethodCallExtractor(Expression e)
		{
			Visit(e);
		}

		/// <summary>
		/// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
		/// </summary>
		/// <param name="node">The expression to visit.</param>
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (_callExpr == null)
				_callExpr = node;
			return base.VisitMethodCall(node);
		}

		/// <summary>
		/// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression" />.
		/// </summary>
		/// <param name="node">The expression to visit.</param>
		protected override Expression VisitNew(NewExpression node)
		{
			if (_newExpr == null)
				_newExpr = node;
			return base.VisitNew(node);
		}

		/// <summary>
		/// Extracts the MethodInfo of the first method call found in the expression.
		/// </summary>
		/// <typeparam name="T">Allows the expression to declare a parameter to aid overload resolution, but doesn't
		/// actually have any material influence on the expression analysis.</typeparam>
		/// <param name="expr">The expression to be read</param>
		/// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
		public static MethodInfo ExtractCalledMethod<T>(Expression<Action<T>> expr)
		{
			return new MethodCallExtractor(expr).CalledMethod;
		}

		/// <summary>
		/// Extracts the MethodInfo of the first method call found in the expression.
		/// </summary>
		/// <param name="expr">The expression to be read</param>
		/// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
		public static MethodInfo ExtractCalledMethod(Expression<Action> expr)
		{
			return new MethodCallExtractor(expr).CalledMethod;
		}

		/// <summary>
		/// Extracts the MethodInfo of the first method call found in the expression.
		/// </summary>
		/// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
		/// when an expression represents a non-void function call</typeparam>
		/// <param name="expr">The expression to be read</param>
		/// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
		public static MethodInfo ExtractCalledMethod<TResult>(Expression<Func<TResult>> expr)
		{
			return new MethodCallExtractor(expr).CalledMethod;
		}

		/// <summary>
		/// Extracts the MethodInfo of the first method call found in the expression.
		/// </summary>
		/// <typeparam name="TInstance">Allows the expression to declare a parameter to aid overload resolution, but doesn't
		/// actually have any material influence on the expression analysis.</typeparam>
		/// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
		/// when an expression represents a non-void function call</typeparam>
		/// <param name="expr">The expression to be read</param>
		/// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
		public static MethodInfo ExtractCalledMethod<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
		{
			return new MethodCallExtractor(expr).CalledMethod;
		}

		/// <summary>
		/// Extracts the ConstructorInfo of the first 'new' statement found in the expression
		/// </summary>
		/// <typeparam name="T">Allows the expression to declare a parameter to aid overload resolution, but doesn't
		/// actually have any material influence on the expression analysis.</typeparam>
		/// <param name="expr">The expression to be analysed.</param>
		/// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
		public static ConstructorInfo ExtractConstructorCall<T>(Expression<Action<T>> expr)
		{
			return new MethodCallExtractor(expr).CalledConstructor;
		}

		/// <summary>
		/// Extracts the ConstructorInfo of the first 'new' statement found in the expression
		/// </summary>
		/// <param name="expr">The expression to be analysed.</param>
		/// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
		public static ConstructorInfo ExtractConstructorCall(Expression<Action> expr)
		{
			return new MethodCallExtractor(expr).CalledConstructor;
		}

		/// <summary>
		/// Extracts the ConstructorInfo of the first 'new' statement found in the expression
		/// </summary>
		/// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
		/// when an expression represents a non-void function call</typeparam>
		/// <param name="expr">The expression to be analysed.</param>
		/// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
		public static ConstructorInfo ExtractConstructorCall<TResult>(Expression<Func<TResult>> expr)
		{
			return new MethodCallExtractor(expr).CalledConstructor;
		}

		/// <summary>
		/// Extracts the ConstructorInfo of the first 'new' statement found in the expression
		/// </summary>
		/// <typeparam name="TInstance">Allows the expression to declare a parameter to aid overload resolution, but doesn't
		/// actually have any material influence on the expression analysis.</typeparam>
		/// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
		/// when an expression represents a non-void function call</typeparam>
		/// <param name="expr">The expression to be analysed.</param>
		/// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
		public static ConstructorInfo ExtractConstructorCall<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
		{
			return new MethodCallExtractor(expr).CalledConstructor;
		}
	}
}