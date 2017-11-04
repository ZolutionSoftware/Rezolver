using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    public class ProjectionTargetBuilder : ExpressionBuilderBase<ProjectionTarget>
    {
        protected override Expression Build(ProjectionTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // functionally the same as the DecoratorTargetBuilder
            var newContext = context.NewContext();
            newContext.Register(target.InputTarget, target.ProjectedType ?? target.InputTarget.DeclaredType);

            return compiler.Build(target.OutputTarget, newContext);
        }
    }
}
