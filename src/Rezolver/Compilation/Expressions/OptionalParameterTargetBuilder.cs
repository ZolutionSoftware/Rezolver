// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building an expression for the <see cref="OptionalParameterTarget"/> target.
    /// </summary>
    public class OptionalParameterTargetBuilder : ExpressionBuilderBase<OptionalParameterTarget>
    {
        /// <summary>
        /// Always returns a <see cref="ConstantExpression"/> which contains the <see cref="OptionalParameterTarget.Value"/>.
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(OptionalParameterTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            // note we ignore the context requested type
            return Expression.Constant(target.Value, target.DeclaredType);
        }
    }
}
