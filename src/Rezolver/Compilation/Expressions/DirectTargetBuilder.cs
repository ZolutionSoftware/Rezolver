using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
    internal class DirectTargetBuilder : ExpressionBuilderBase<IDirectTarget>
    {
        protected override Expression Build(IDirectTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Expression.Call(
                Expression.Constant(target), Methods.IDirectTarget_GetValue_Method
                );
        }
    }
}
