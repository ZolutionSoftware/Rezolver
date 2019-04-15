// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Expression builder for the <see cref="VariantMatchTarget"/>
    /// </summary>
    public class VariantMatchTargetBuilder : ExpressionBuilderBase<VariantMatchTarget>
    {
        /// <summary>
        /// Builds the conversion expression represented by the <paramref name="target"/>
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.</param>
        protected override Expression Build(VariantMatchTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // Here, the target type is *key*, especially for singletons
            

            return Expression.Convert(compiler.Build(target.Target,
                context.NewContext(target.RegisteredType, scopeBehaviourOverride: context.ScopeBehaviourOverride)), target.RequestedType);
        }
    }
}
