using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq.Expressions
{
    internal static class LambdaCompilerAbstraction
    {
        public static Func<T> CompileForRezolver<T>(this Expression<Func<T>> lambda)
        {
            var result = lambda.CompileFast();
            return result;
        }

        public static Func<T1, TResult> CompileForRezolver<T1, TResult>(this Expression<Func<T1, TResult>> lambda)
        {
            var result = lambda.CompileFast();
            return result;
        }

        public static Func<T1, T2, TResult> CompileForRezolver<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> lambda)
        {
            var result = lambda.CompileFast();
            return result;
        }

    }
}
