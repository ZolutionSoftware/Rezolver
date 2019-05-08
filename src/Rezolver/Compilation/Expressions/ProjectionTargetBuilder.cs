// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq.Expressions;
using Rezolver.Runtime;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Specialised expression builder for the <see cref="ProjectionTarget"/> target type.
    /// </summary>
    public class ProjectionTargetBuilder : ExpressionBuilderBase<ProjectionTarget>
    {
        /// <summary>
        /// Builds an expression for the passed <paramref name="target"/>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <param name="compiler"></param>
        /// <returns></returns>
        protected override Expression Build(ProjectionTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // functionally the same as the DecoratorTargetBuilder
            var newContext = context.NewContext(target.ImplementationType);
            newContext.Register(target.InputTarget, target.InputType ?? target.InputTarget.DeclaredType);

            // projection target acts as an anchor for the target it wraps - this allows a single registered
            // target which is either a singleton or scoped to be reused for multiple input targets.
            newContext.SetOption(new TargetIdentityOverride(target.Id), target.DeclaredType);
            return compiler.Build(target.OutputTarget, newContext);
        }
    }
}
