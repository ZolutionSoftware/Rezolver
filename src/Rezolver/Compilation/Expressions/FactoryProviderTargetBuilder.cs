// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Special compiler for <see cref="ITarget"/> instances which are also <see cref="IFactoryProvider"/> instances.
    /// 
    /// Fetches the <see cref="IFactoryProvider.Factory"/> and builds an expression which represents invoking that factory
    /// with a new resolve context.
    /// </summary>
    public class FactoryProviderTargetBuilder : ExpressionBuilderBase<IFactoryProvider>
    {
        /// <summary>
        /// Implementation of <see cref="ExpressionBuilderBase{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// </summary>
        /// <param name="target">The instance whose <see cref="IFactoryProvider.Factory"/> is to be invoked by the returned expression.</param>
        /// <param name="context">The compilation context</param>
        /// <param name="compiler">The compiler for which the expression is being built.</param>
        /// <returns>The created expression.</returns>
        protected override Expression Build(IFactoryProvider target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var factory = target.Factory;

            return Expression.Invoke(
                    Expression.Constant(factory, typeof(Func<ResolveContext, object>)),
                    Methods.CallResolveContext_New_Type(context.ResolveContextParameterExpression,
                        Expression.Constant(context.TargetType))
                );
        }
    }
}
