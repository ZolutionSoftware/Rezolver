// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq.Expressions;
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
            TypeAndTargetId id = new TypeAndTargetId(context.TargetType ?? target.DeclaredType, targetIdOverride ?? target.Id);

            var lazy = holder.GetLazy(id);

            if (lazy == null)
            {
                lazy = holder.GetLazy(
                    target, 
                    id, 
                    compiler.CompileTargetStrong(
                        target.InnerTarget,
                        context.NewContext(
                            context.TargetType ?? target.DeclaredType,
                            // this override is important - when forcing into the root-scope, as we do
                            // for singletons, 'explicit' means absolutely nothing.  So, instead of allowing
                            // our child target to choose, we explicitly ensure that all instances are implicitly 
                            // tracked within the root scope, if it is one which can track instances.
                            scopeBehaviourOverride: ScopeBehaviour.Implicit,
                            scopePreferenceOverride: ScopePreference.Root)),
                    context);
            }

            return Expression.Call(
                Expression.Constant(lazy),
                lazy.GetType().GetMethod("Resolve"),
                context.ResolveContextParameterExpression);
        }

        /// <summary>
        /// Overrides the base to prevent additional scoping information being added to the expression returned by <see cref="Build(SingletonTarget, IExpressionCompileContext, IExpressionCompiler)"/>
        /// </summary>
        /// <param name="builtExpression"></param>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="compiler"></param>
        /// <returns></returns>
        protected override Expression ApplyScoping(Expression builtExpression, ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return builtExpression;
        }
    }
}
