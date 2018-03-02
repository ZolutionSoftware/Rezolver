// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rezolver.Compilation.Expressions
{
    internal class TargetExpressionRewriter : ExpressionVisitor
    {
        readonly IExpressionCompileContext _sourceCompileContext;
        readonly IExpressionCompiler _compiler;

        public TargetExpressionRewriter(IExpressionCompiler compiler, IExpressionCompileContext context)
        {
            this._compiler = compiler;
            this._sourceCompileContext = context;
        }

        public override Expression Visit(Expression node)
        {
            if (node != null)
            {
                if (node.NodeType == ExpressionType.Extension && node is TargetExpression te)
                {
                    return this._compiler.Build(te.Target, this._sourceCompileContext.NewContext(te.Type));
                }
            }

            return base.Visit(node);
        }
    }
}
