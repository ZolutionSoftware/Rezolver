﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Interface for an object which is responsible for coordinating the production of expressions for targets
    /// during the compilation phase.
    ///
    /// Objects implementing this are expected to be implementations of <see cref="ITargetCompiler"/>; this library
    /// provides the one implementation, too: <see cref="ExpressionCompiler"/>.
    /// </summary>
    /// <remarks>
    /// All expressions are built to be called by a container's <see cref="Container.Resolve(ResolveContext)"/>.
    ///
    /// Note that the <see cref="Build(ITarget, IExpressionCompileContext)"/> method declared here is effectively an
    /// analogue to the <see cref="IExpressionBuilder.Build(ITarget, IExpressionCompileContext, IExpressionCompiler)"/>.  Indeed, the default
    /// implementation resolves <see cref="IExpressionBuilder"/> instances to delegate the building of expressions.
    /// </remarks>
    public interface IExpressionCompiler : ITargetCompiler
    {
        /// <summary>
        /// Gets an unoptimised expression containing the logic required to create or fetch an instance of the <paramref name="target"/>'s
        /// <see cref="ITarget.DeclaredType"/> when invoked for a particular <see cref="ResolveContext"/>.
        ///
        /// Use this method if you want the raw expression for a target (possibly when integrating it into your own expressions during custom
        /// compilation).
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="context">Contains the supporting expressions (shared locals, shared expressions,
        /// ResolveContext expression etc) that have been used in the generation of the expression.</param>
        /// <remarks>
        ///
        /// If you want to build the optimised code for the passed target, you should use the
        /// <see cref="ExpressionCompilerBuildExtensions.BuildResolveLambda(IExpressionCompiler, ITarget, IExpressionCompileContext)"/>
        /// extension method, which uses this method and then passes the result to the
        /// <see cref="BuildObjectFactoryLambda(Expression, IExpressionCompileContext)"/> function also defined on
        /// this interface.</remarks>
        Expression Build(ITarget target, IExpressionCompileContext context);

        /// <summary>
        /// This function optimises and prepares an expression that's (most likely) previously been
        /// produced by the <see cref="Build(ITarget, IExpressionCompileContext)"/> function into a
        /// lambda expression which can be compiled into a delegate and executed; or quoted inside
        /// another expression as a callback.
        ///
        /// The <see cref="IExpressionCompileContext.ResolveContextParameterExpression"/> of the
        /// <paramref name="context"/> will be used to define the single parameter for the lambda that
        /// is created.
        /// </summary>
        /// <param name="expression">Expression to be optimised and used as the body of the lambda.
        /// Any expression produced by the <see cref="Build(ITarget, IExpressionCompileContext)"/> method
        /// must be compatible with this.</param>
        /// <param name="context">Contains the supporting expressions (shared locals, shared expressions,
        /// ResolveContext expression etc) that have been used in the generation of the expression.</param>
        /// <returns>A lambda expression which, when compiled and executed, will produce an object
        /// consistent with the <see cref="ITarget"/> from which the code was produced.</returns>
        Expression<Func<ResolveContext, object>> BuildObjectFactoryLambda(Expression expression, IExpressionCompileContext context);

        /// <summary>
        /// Builds a delegate whose type is `Func{<see cref="ResolveContext"/>, {Type}}` where `{Type}` is equal
        /// to the static <see cref="Expression.Type"/> of the <paramref name="expression"/>
        /// </summary>
        /// <param name="expression">Expression to be optimised and used as the body of the lambda.</param>
        /// <param name="context">Contains the supporting expressions (shared locals, shared expressions,
        /// ResolveContext expression etc) that have been used in the generation of the expression.</param>
        /// <returns>A lambda expression which is strongly type for <see cref="Func{T, TResult}"/> with `TResult`
        /// equal to the type of the input expression; and `T` equal to <see cref="ResolveContext"/>.</returns>
        LambdaExpression BuildStrongFactoryLambda(Expression expression, IExpressionCompileContext context);
    }
}