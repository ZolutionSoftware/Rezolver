using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
    public class CompiledTargetBuilder : ExpressionBuilderBase<ICompiledTarget>
    {
        protected override Expression Build(ICompiledTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Expression.Call(
                    Expression.Constant(target),
                    ICompiledTarget_GetObject_Method,
                    CallResolveContext_New(context.ResolveContextParameterExpression,
                        Expression.Constant(context.TargetType))
                );

        }
    }
}
