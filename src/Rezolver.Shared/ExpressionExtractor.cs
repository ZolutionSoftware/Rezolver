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
    internal sealed class ExpressionExtractor : ExpressionVisitor
    {
        internal ExpressionExtractor(Expression e)
        {
            this.Visit(e);
        }

        internal MethodCallExpression CallExpression { get; private set; }

        internal NewExpression NewExpression { get; private set; }

        internal MemberExpression MemberExpression { get; private set; }

        internal MethodInfo CalledMethod => this.CallExpression?.Method;

        internal ConstructorInfo CalledConstructor => this.NewExpression?.Constructor;

        internal MemberInfo Member => this.MemberExpression?.Member;

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (this.CallExpression == null)
            {
                this.CallExpression = node;
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.NewExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        protected override Expression VisitNew(NewExpression node)
        {
            if (this.NewExpression == null)
            {
                this.NewExpression = node;
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (this.MemberExpression == null)
            {
                this.MemberExpression = node;
            }

            return base.VisitMember(node);
        }
    }
}