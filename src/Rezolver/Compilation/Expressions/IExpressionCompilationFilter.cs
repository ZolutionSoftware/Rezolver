using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
