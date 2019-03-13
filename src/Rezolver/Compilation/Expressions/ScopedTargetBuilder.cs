// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building expressions for <see cref="ScopedTarget"/> targets.
    /// </summary>
    public class ScopedTargetBuilder : ExpressionBuilderBase<ScopedTarget>
    {
        private readonly ConstructorInfo _argExceptionCtor =
            Extract.Constructor(() => new ArgumentException("", ""));

        /// <summary>
        /// Builds an expression for the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(ScopedTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // all we need to do is force the inner target's scope behaviour to None - and this builder's
            // base code will ensure that the whole resulting expression is converted into an explicitly scoped one

            // note that this scope deactivation is only in place for this one target - if it has any child targets then
            // scoping behaviour for those returns to normal if compiled with a new context (which they always should be)

            return compiler.Build(target.InnerTarget, context.NewContext(scopeBehaviourOverride: ScopeBehaviour.None));
        }
    }
}
