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
            var holder = context.ResolveContext.Resolve<SingletonTarget.SingletonContainer>();
            int? targetIdOverride = context.GetOption<TargetIdentityOverride>(context.TargetType ?? target.DeclaredType);

            var compiled = compiler.CompileTarget(
                    target.InnerTarget,
                    context.NewContext(
                        context.TargetType ?? target.DeclaredType,
                        // this override is important - when forcing into the root-scope, as we do
                        // for singletons, 'explicit' means absolutely nothing.  So, instead of allowing
                        // our child target to choose, we explicitly ensure that all instances are implicitly 
                        // tracked within the root scope, if it is one which can track instances.
                        scopeBehaviourOverride: ScopeBehaviour.Implicit,
                        scopePreferenceOverride: ScopePreference.Root));
#if !USEDYNAMIC

            return Expression.Convert(Expression.Call(
                Expression.Constant(holder),
                nameof(SingletonTarget.SingletonContainer.GetObject),
                null,
                context.ResolveContextParameterExpression,
                Expression.Constant(context.TargetType, typeof(Type)),
                Expression.Constant(targetIdOverride ?? target.Id),
                Expression.Constant(compiled)), context.TargetType);
#else
            var entryType = holder.GetEntryType(compiled, targetIdOverride ?? target.Id, context.TargetType);
            return Expression.Call(entryType.GetMethod("Resolve", BindingFlags.Static | BindingFlags.Public), context.ResolveContextParameterExpression);
#endif
        }

        protected override Expression ApplyScoping(Expression builtExpression, ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return builtExpression;
        }
    }
}
