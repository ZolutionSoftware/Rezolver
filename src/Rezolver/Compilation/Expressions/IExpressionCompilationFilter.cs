// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// When the <see cref="ExpressionCompiler"/> is asked to compile a given target for the given context,
    /// the compiler will first attempt to obtain an instance of this type as an option from the context.
    /// 
    /// If one is set, then it will be consulted to see if it can offer up an expression for the given target
    /// instead of the compiler doing its normal process.
    /// </summary>
    [Obsolete("See also ExpressionCompilationFilters", true)]
    internal interface IExpressionCompilationFilter
    {
        Expression Intercept(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler);
    }
}
