// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

            return Expression.Constant(holder.GetObject(context, target,
                c => compiler.CompileTarget(
                    target.InnerTarget,
                    c.NewContext(
                        context.TargetType ?? target.DeclaredType,
                        scopeBehaviourOverride: ScopeBehaviour.None,
                        scopePreferenceOverride: ScopePreference.Root))));
        }
    }
}
