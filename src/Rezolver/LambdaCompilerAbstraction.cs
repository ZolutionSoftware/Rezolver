// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Compilation.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq.Expressions
{
    internal static class LambdaCompilerAbstraction
    {
        //private class LambdaParameterIsolator : ExpressionVisitor
        //{
        //    public static readonly LambdaParameterIsolator Instance = new LambdaParameterIsolator();

        //    private static int _counter = 0;
        //    protected override Expression VisitLambda<T>(Expression<T> node)
        //    {
        //        var replacements =
        //        node.Parameters.Select((p, i) => new ExpressionReplacement(p, Expression.Parameter(p.Type, $"p{i}_{++_counter}"))).ToArray();

        //        var replacer = new ExpressionSwitcher(replacements);

        //        var replaced = (Expression<T>)replacer.Visit(node);

        //        // then recurse into the body
        //        return Expression.Lambda<T>(base.Visit(replaced.Body), replaced.Parameters);
        //    }
        //}

        public static Func<T> CompileForRezolver<T>(this Expression<Func<T>> lambda)
        {
            return lambda.Compile();
        }

        public static Func<T1, TResult> CompileForRezolver<T1, TResult>(this Expression<Func<T1, TResult>> lambda)
        {
            // note here: all lambdas' parameters are replaced 

            // var newLambda = (Expression<Func<T1, TResult>>)LambdaParameterIsolator.Instance.Visit(lambda);

            return lambda.Compile();
        }

        public static Func<T1, T2, TResult> CompileForRezolver<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> lambda)
        {
            return lambda.Compile();
        }

    }
}
