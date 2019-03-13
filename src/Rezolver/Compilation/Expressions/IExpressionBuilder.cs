// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// The interface for an object that produces expressions (which can be compiled to delegates)
    /// from <see cref="ITarget"/> instances.
    /// </summary>
    public interface IExpressionBuilder
    {
        /// <summary>
        /// Determines whether this instance can build an expression for the specified target type.
        /// </summary>
        /// <param name="targetType">The type of target.</param>
        bool CanBuild(Type targetType);
        /// <summary>
        /// Builds an expression for the specified target.
        /// </summary>
        /// <param name="target">The target for which an expression is to be built</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">Optional. The compiler that's requesting the expression; and which can be used
        /// to compile other targets within the target.  If not provided, then the builder should attempt to
        /// fetch the compiler from the context; or throw an exception if it is required but not provided and
        /// cannot be resolved fromm the context.
        /// </param>
        /// <remarks>When invoked by the <see cref="ExpressionCompiler"/> class, the <paramref name="compiler"/>
        /// parameter will always be provided.</remarks>
        Expression Build(ITarget target, IExpressionCompileContext context, IExpressionCompiler compiler = null);
    }
}
