// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Extensions for <see cref="IExpressionCompiler"/>.
    /// </summary>
    public static class ExpressionCompilerBuildExtensions
    {
        /// <summary>
        /// This method is a shortcut for building a lambda expression directly from an <see cref="ITarget" />.
        /// It calls <see cref="IExpressionCompiler.Build(ITarget, IExpressionCompileContext)" /> and passes the result to
        /// <see cref="IExpressionCompiler.BuildObjectFactoryLambda(Expression, IExpressionCompileContext)" />, which should yield an
        /// optimised lambda expression for the expression produced from the target which can then be
        /// compiled and used as the factory for that target.
        /// </summary>
        /// <param name="compiler">The compiler.</param>
        /// <param name="target">The target.</param>
        /// <param name="context">The current compilation context.</param>
        public static Expression<Func<ResolveContext, object>> BuildResolveLambda(this IExpressionCompiler compiler, ITarget target, IExpressionCompileContext context)
        {
            if(compiler == null) throw new ArgumentNullException(nameof(compiler));
            if(target == null) throw new ArgumentNullException(nameof(target));

            var expression = compiler.Build(target, context);
            return compiler.BuildObjectFactoryLambda(expression, context);
        }

        /// <summary>
        /// Similar to <see cref="BuildResolveLambda(IExpressionCompiler, ITarget, IExpressionCompileContext)"/>, except this builds a
        /// lambda whose type is a strongly-typed delegate instead of object - i.e. Func{ResolveContext, T}
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="target"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LambdaExpression BuildResolveLambdaStrong(this IExpressionCompiler compiler, ITarget target, IExpressionCompileContext context)
        {
            if (compiler == null) throw new ArgumentNullException(nameof(compiler));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var expression = compiler.Build(target, context);
            return compiler.BuildStrongFactoryLambda(expression, context);
        }
    }
}
