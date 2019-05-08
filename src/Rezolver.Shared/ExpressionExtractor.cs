// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

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
    internal sealed class ExpressionExtractor : ExpressionVisitor
    {
        internal ExpressionExtractor(Expression e)
        {
            Visit(e);
        }

        internal MethodCallExpression CallExpression { get; private set; }

        internal NewExpression NewExpression { get; private set; }

        internal MemberExpression MemberExpression { get; private set; }

        internal MethodInfo CalledMethod => CallExpression?.Method;

        internal ConstructorInfo CalledConstructor => NewExpression?.Constructor;

        internal MemberInfo Member => MemberExpression?.Member;

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (CallExpression == null)
            {
                CallExpression = node;
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitNew(NewExpression node)
        {
            if (NewExpression == null)
            {
                NewExpression = node;
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (MemberExpression == null)
            {
                MemberExpression = node;
            }

            return base.VisitMember(node);
        }
    }
}