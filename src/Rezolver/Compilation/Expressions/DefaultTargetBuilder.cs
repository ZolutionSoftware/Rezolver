// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An <see cref="IExpressionBuilder"/> specialised for building the expression for the <see cref="DefaultTarget"/> target.
    ///
    /// Essentially, it just calls <see cref="Expression.Default(Type)"/> for the <see cref="DefaultTarget.DeclaredType"/>.
    /// </summary>
    public class DefaultTargetBuilder : ExpressionBuilderBase<DefaultTarget>
    {
        /// <summary>
        /// Builds an expression from the specified target for the given <see cref="T:Rezolver.Compilation.ICompileContext" />
        /// </summary>
        /// <param name="target">The target whose expression is to be built.</param>
        /// <param name="context">The compilation context.</param>
        /// <param name="compiler">The expression compiler to be used to build any other expressions for targets
        /// which might be required by the <paramref name="target" />.  Note that unlike on the interface, where this
        /// parameter is optional, this will always be provided</param>
        protected override Expression Build(DefaultTarget target, IExpressionCompileContext context, IExpressionCompiler compiler)
        {
            return Expression.Default(target.DeclaredType);
        }
    }
}
