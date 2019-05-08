// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building the expression for the <see cref="ResolvedTarget"/> target.
    /// </summary>
    public class RezolvedTargetBuilder : ExpressionBuilderBase<ResolvedTarget>
    {
        /// <summary>
        /// Builds an expression for the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected override Expression Build(ResolvedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var staticTarget = target.Bind(context);

            Expression staticExpr;

            if (staticTarget != null)
            {
                staticExpr = compiler.Build(staticTarget, context.NewContext(target.DeclaredType));

                if (staticExpr == null)
                    throw new InvalidOperationException(string.Format(ExceptionResources.TargetReturnedNullExpressionFormat, staticTarget.GetType(), context.TargetType));

                if (staticExpr.Type != target.DeclaredType)
                    staticExpr = Expression.Convert(staticExpr, target.DeclaredType);
            }
            else
            {
                // this should generate a missing dependency exception if executed
                // or, might actually yield a result if registrations have been added
                // after the expression is compiled.
                staticExpr = Methods.CallResolveContext_Resolve_Strong_Method(
                    context.ResolveContextParameterExpression,
                    target.DeclaredType);
            }

            return staticExpr;
        }
    }
}
