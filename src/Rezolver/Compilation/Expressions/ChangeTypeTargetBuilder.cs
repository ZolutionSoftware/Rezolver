// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Implementation of <see cref="IExpressionBuilder"/> specialised to build expressions for the
    /// <see cref="ChangeTypeTarget"/>
    ///
    /// This always produces a conversion expression (i.e. cast or box/unbox)
    /// </summary>
    public class ChangeTypeTargetBuilder : ExpressionBuilderBase<ChangeTypeTarget>
    {
        /// <summary>
        /// Builds the conversion expression represented by the <paramref name="target"/>
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.</param>
        protected override Expression Build(ChangeTypeTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // build the inner target's expression; and wrap it in a conversion expression for the
            // target type of the ChangeTypeTarget.
            // note that if the compilation context was overriding the scoping behaviour before - then we pass that through,
            // because this target defaults to 'None'
            return Expression.Convert(compiler.Build(target.InnerTarget,
                context.NewContext(target.InnerTarget.DeclaredType, scopeBehaviourOverride: context.ScopeBehaviourOverride)), target.DeclaredType);
        }
    }
}
