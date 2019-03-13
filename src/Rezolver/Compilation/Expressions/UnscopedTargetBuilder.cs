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
    /// Builder for the <see cref="UnscopedTarget"/>.
    /// </summary>
    public class UnscopedTargetBuilder : ExpressionBuilderBase<UnscopedTarget>
    {
        /// <summary>
        /// overrides the base method to block all automatic scoping code from the expression being built.
        /// </summary>
        protected override Expression ApplyScoping(Expression builtExpression, ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // forcibly disable any scoping code for the expression that's built if somehow it sneaks through
            return builtExpression;
        }

        /// <summary>
        /// Builds an expression for the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(UnscopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // force scoping off for the inner target
            return compiler.Build(target.Inner, context.NewContext(scopeBehaviourOverride: ScopeBehaviour.None));
        }
    }
}
