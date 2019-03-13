// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
