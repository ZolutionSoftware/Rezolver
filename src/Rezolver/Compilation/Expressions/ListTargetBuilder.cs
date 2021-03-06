﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Collections.Generic;
using System.Linq.Expressions;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building expressions for the <see cref="ListTarget"/> target.
    /// </summary>
    public class ListTargetBuilder : ExpressionBuilderBase<ListTarget>
    {
        /// <summary>
        /// Builds an expression which either represents creating an array or a list of objects using an
        /// enumerable of targets from the <paramref name="target"/>'s <see cref="ListTarget.Items"/>.
        ///
        /// The target's <see cref="ListTarget.AsArray"/> flag is used to determine which expression to build.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(ListTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var items = new List<Expression>();

            foreach(var itemTarget in target.Items)
            {
                items.Add(compiler.Build(itemTarget, context.NewContext(target.ElementType)));
            }

            var arrayExpr = Expression.NewArrayInit(target.ElementType, items);

            if (target.AsArray)
            {
                return arrayExpr;
            }
            else
            {
                return Expression.New(target.ListConstructor, arrayExpr);
            }
        }
    }
}
