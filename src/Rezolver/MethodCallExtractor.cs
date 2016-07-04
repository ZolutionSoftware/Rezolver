// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	public sealed class MethodCallExtractor : ExpressionVisitor
	{
		private MethodCallExpression _callExpr;
		private NewExpression _newExpr;

		public MethodCallExpression CallExpression
		{
			get { return _callExpr; }
		}

		public MethodInfo CalledMethod
		{
			get
			{
				return _callExpr != null ? _callExpr.Method : null;
			}
		}

		public ConstructorInfo CalledConstructor
		{
			get
			{
				return _newExpr.Constructor;
			}
		}

		private MethodCallExtractor(Expression e)
		{
			Visit(e);
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (_callExpr == null)
				_callExpr = node;
			return base.VisitMethodCall(node);
		}

		protected override Expression VisitNew(NewExpression node)
		{
			if (_newExpr == null)
				_newExpr = node;
			return base.VisitNew(node);
		}

		public static MethodInfo ExtractCalledMethod<T>(Expression<Action<T>> expr)
		{
			var visitor = new MethodCallExtractor(expr);

			return visitor.CalledMethod;
		}

		public static MethodInfo ExtractCalledMethod(Expression<Action> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledMethod;
		}

		public static MethodInfo ExtractCalledMethod<TResult>(Expression<Func<TResult>> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledMethod;
		}

		public static MethodInfo ExtractCalledMethod<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledMethod;
		}

		public static ConstructorInfo ExtractConstructorCall<T>(Expression<Action<T>> expr)
		{
			var visitor = new MethodCallExtractor(expr);

			return visitor.CalledConstructor;
		}

		public static ConstructorInfo ExtractConstructorCall(Expression<Action> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledConstructor;
		}

		public static ConstructorInfo ExtractConstructorCall<TResult>(Expression<Func<TResult>> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledConstructor;
		}

		public static ConstructorInfo ExtractConstructorCall<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
		{
			var visitor = new MethodCallExtractor(expr);
			return visitor.CalledConstructor;
		}
	}
}