using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Rezolver.Runtime;

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
