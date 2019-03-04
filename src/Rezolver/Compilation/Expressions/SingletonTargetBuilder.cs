// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for the building the expression for a <see cref="SingletonTarget"/> target.
    /// </summary>
    public class SingletonTargetBuilder : ExpressionBuilderBase<SingletonTarget>
    {
        /// <summary>
        /// Builds an expression for the given <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(SingletonTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            var holder = context.ResolveContext.Container.Resolve<SingletonTarget.SingletonContainer>();
            int? targetIdOverride = context.GetOption<TargetIdentityOverride>(context.TargetType ?? target.DeclaredType);

            return Expression.Constant(
                holder.GetObject(
                    target, 
                    context,
                    targetIdOverride ?? target.Id,
                    GetCompiled));

            ICompiledTarget GetCompiled(SingletonTarget singleton, IExpressionCompileContext c) => 
                compiler.CompileTarget(
                    singleton.InnerTarget, 
                    c.NewContext(
                        c.TargetType ?? singleton.DeclaredType, 
                        scopeBehaviourOverride: ScopeBehaviour.Implicit,
                        scopePreferenceOverride: ScopePreference.Root));
        }
    }
}
