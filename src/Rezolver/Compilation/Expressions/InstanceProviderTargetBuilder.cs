// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Special compiler for instances of <see cref="IInstanceProvider"/> which binds to the <see cref="IInstanceProvider.GetInstance(ResolveContext)"/>
    /// method in the expression.
    /// </summary>
    public class InstanceProviderTargetBuilder : ExpressionBuilderBase<IInstanceProvider>
    {
        /// <summary>
        /// Implementation of <see cref="ExpressionBuilderBase{TTarget}.Build(TTarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// </summary>
        /// <param name="target">The instance whose <see cref="IInstanceProvider.GetInstance(ResolveContext)"/> method is to be called
        /// by the returned expression.</param>
        /// <param name="context">The compilation context</param>
        /// <param name="compiler">The compiler for which the expression is being built.</param>
        /// <returns>The created expression</returns>
        protected override Expression Build(IInstanceProvider target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Expression.Call(
                    Expression.Constant(target),
                    Methods.IInstanceProvider_GetInstance_Method,
                    Methods.CallResolveContext_New_Type(context.ResolveContextParameterExpression,
                        Expression.Constant(context.TargetType))
                );
        }
    }
}
