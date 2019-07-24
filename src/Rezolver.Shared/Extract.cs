// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
    internal static class Extract
    {
        internal static ConstructorInfo GenericConstructor<T>(Expression<Func<T>> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor?.ToGenericTypeDefCtor();
        }

        internal static ConstructorInfo GenericConstructor<TInstance, T>(Expression<Func<TInstance, T>> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor?.ToGenericTypeDefCtor();
        }

        internal static MethodInfo GenericTypeMethod<T>(Expression<Func<T>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod?.ToGenericTypeDefMethod();
        }

        internal static MethodInfo GenericTypeMethod<TInstance, T>(Expression<Func<TInstance, T>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod?.ToGenericTypeDefMethod();
        }

        internal static MethodInfo GenericTypeMethod(Expression<Action> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod?.ToGenericTypeDefMethod();
        }

        internal static MethodInfo GenericTypeMethod<TInstance>(Expression<Action<TInstance>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod?.ToGenericTypeDefMethod();
        }

        internal static MemberInfo Member<TInstance, TMember>(Expression<Func<TInstance, TMember>> expr)
        {
            var extractor = new ExpressionExtractor(expr);
            if (extractor.Member != null)
            {
                if (expr.Body == extractor.MemberExpression && extractor.MemberExpression.Expression == expr.Parameters[0])
                {
                    return extractor.Member;
                }
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
        internal static MethodInfo Method<T>(Expression<Action<T>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod;
        }

        /// <summary>
        /// Extracts the MethodInfo of the first method call found in the expression.
        /// </summary>
        /// <param name="expr">The expression to be read</param>
        /// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
        internal static MethodInfo Method(Expression<Action> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod;
        }

        /// <summary>
        /// Extracts the MethodInfo of the first method call found in the expression.
        /// </summary>
        /// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
        /// when an expression represents a non-void function call</typeparam>
        /// <param name="expr">The expression to be read</param>
        /// <returns>A MethodInfo representing the method - if the expression contains a method call; otherwise <c>null</c></returns>
        internal static MethodInfo Method<TResult>(Expression<Func<TResult>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod;
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
        internal static MethodInfo Method<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
        {
            return new ExpressionExtractor(expr).CalledMethod;
        }

        /// <summary>
        /// Extracts the ConstructorInfo of the first 'new' statement found in the expression
        /// </summary>
        /// <typeparam name="T">Allows the expression to declare a parameter to aid overload resolution, but doesn't
        /// actually have any material influence on the expression analysis.</typeparam>
        /// <param name="expr">The expression to be analysed.</param>
        /// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
        internal static ConstructorInfo Constructor<T>(Expression<Action<T>> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor;
        }

        /// <summary>
        /// Extracts the ConstructorInfo of the first 'new' statement found in the expression
        /// </summary>
        /// <param name="expr">The expression to be analysed.</param>
        /// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
        internal static ConstructorInfo Constructor(Expression<Action> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor;
        }

        /// <summary>
        /// Extracts the ConstructorInfo of the first 'new' statement found in the expression
        /// </summary>
        /// <typeparam name="TResult">Return type of the expression.  Not actually used in any way, it's just here to allow overload resolution
        /// when an expression represents a non-void function call</typeparam>
        /// <param name="expr">The expression to be analysed.</param>
        /// <returns>A ConstructorInfo representing the constructor being called in the expression; otherwise <c>null</c> if not found.</returns>
        internal static ConstructorInfo Constructor<TResult>(Expression<Func<TResult>> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor;
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
        internal static ConstructorInfo Constructor<TInstance, TResult>(Expression<Func<TInstance, TResult>> expr)
        {
            return new ExpressionExtractor(expr).CalledConstructor;
        }
    }
}
