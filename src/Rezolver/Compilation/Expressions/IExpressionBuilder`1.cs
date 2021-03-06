﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// Interface for an object that produces expressions from instances of <typeparamref name="TTarget"/>.
    ///
    /// This is a generic extension to the <see cref="IExpressionBuilder"/> interface.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target.</typeparam>
    /// <seealso cref="Rezolver.Compilation.Expressions.IExpressionBuilder" />
    public interface IExpressionBuilder<in TTarget> : IExpressionBuilder
    {
        /// <summary>
        /// Builds an expression from the specified target.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">Optional. The compiler that's requesting the expression; and which can be used
        /// to compile other targets within the target.  If not provided, then the builder should attempt to
        /// fetch the compiler from the context; or throw an exception if it is required but not provided and
        /// cannot be resolved fromm the context.
        /// </param>
        /// <remarks>When invoked by the <see cref="ExpressionCompiler"/> class, the <paramref name="compiler"/>
        /// parameter will always be provided.</remarks>
        Expression Build(TTarget target, IExpressionCompileContext context, IExpressionCompiler compiler = null);
    }
}
