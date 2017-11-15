// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
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
    internal sealed class MethodCallExtractor : ExpressionVisitor
    {
        internal MethodCallExpression CallExpression { get; private set; }
        internal NewExpression NewExpression { get; private set; }
        internal MemberExpression MemberExpression { get; private set; }

        internal MethodInfo CalledMethod => CallExpression?.Method;

        internal ConstructorInfo CalledConstructor => NewExpression?.Constructor;

        internal MemberInfo Member => MemberExpression?.Member;

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
			if (CallExpression == null)
				CallExpression = node;
			return base.VisitMethodCall(node);
		}

		/// <summary>
		/// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression" />.
		/// </summary>
		/// <param name="node">The expression to visit.</param>
		protected override Expression VisitNew(NewExpression node)
		{
			if (NewExpression == null)
				NewExpression = node;
			return base.VisitNew(node);
		}

        protected override Expression VisitMember(MemberExpression node)
        {
            if (MemberExpression == null)
                MemberExpression = node;

            return base.VisitMember(node);
        }

        public static MemberInfo ExtractMemberAccess<TInstance, TMember>(Expression<Func<TInstance, TMember>> expr)
        {
            var extractor = new MethodCallExtractor(expr);
            if(extractor.Member != null)
            {
                if (expr.Body == extractor.MemberExpression && extractor.MemberExpression.Expression == expr.Parameters[0])
                    return extractor.Member;
            }
            return null;
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