using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// FastExpressionCompiler is faster for more complex delegates.
#if false   //!MAXCOMPAT
using FastExpressionCompiler;
#endif
namespace System.Linq.Expressions
{
    internal static class LambdaCompilerAbstraction
    {
        public static Func<T> CompileForRezolver<T>(this Expression<Func<T>> lambda)
        {
#if true //MAXCOMPAT
            return lambda.Compile();
#else
            return lambda.CompileFast();
#endif

        }

        public static Func<T1, TResult> CompileForRezolver<T1, TResult>(this Expression<Func<T1, TResult>> lambda)
        {
#if true
            return lambda.Compile();
#else
            return lambda.CompileFast();
#endif

        }

        public static Func<T1, T2, TResult> CompileForRezolver<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> lambda)
        {
#if true
            return lambda.Compile();
#else
            return lambda.CompileFast();
#endif

        }

    }
}
