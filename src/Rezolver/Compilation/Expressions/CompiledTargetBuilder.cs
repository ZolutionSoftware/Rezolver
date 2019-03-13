// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Special compiler for <see cref="ITarget"/> instances which are also <see cref="ICompiledTarget"/> instances.
    ///
    /// This build is only used when no 'better' builder is available for the target's type.  So, the <see cref="ObjectTargetBuilder"/>
    /// will be used for <see cref="Rezolver.Targets.ObjectTarget"/> instead of this one, even though that class also implements
    /// <see cref="ICompiledTarget"/>.
    /// </summary>
    /// <remarks>This builder creates an expression which explicitly calls the <see cref="ICompiledTarget.GetObject(ResolveContext)"/>
    /// method of the target, with an <see cref="ResolveContext"/> which naturally flows from the one created for the wider
    /// resolve call to the container.</remarks>
    public class CompiledTargetBuilder : ExpressionBuilderBase<ICompiledTarget>
    {
        /// <summary>
        /// Builds an expression which calls the <see cref="ICompiledTarget.GetObject(ResolveContext)"/> of the passed <paramref name="target"/>
        /// </summary>
        /// <param name="target">The target whose <see cref="ICompiledTarget.GetObject(ResolveContext)"/> method is to be called by the expression
        /// that is returned from this builder.</param>
        /// <param name="context">The compilation context</param>
        /// <param name="compiler">The compiler for which the expression is being built.</param>
        /// <returns>The created expression.</returns>
        protected override Expression Build(ICompiledTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Expression.Call(
                    Expression.Constant(target),
                    Methods.ICompiledTarget_GetObject_Method,
                    Methods.CallResolveContext_New_Type(context.ResolveContextParameterExpression,
                        Expression.Constant(context.TargetType))
                );
        }
    }
}
