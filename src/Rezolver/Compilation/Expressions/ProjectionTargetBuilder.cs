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
    public class ProjectionTargetBuilder : ExpressionBuilderBase<ProjectionTarget>
    {
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
