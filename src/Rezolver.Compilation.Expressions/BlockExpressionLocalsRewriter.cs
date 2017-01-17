// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver.Compilation.Expressions
{
  /// <summary>
  /// Used to rewrite any block expressions to strip out any locals that have been lifted
  /// out to an outer scope.
  /// 
  /// This prevents additional instances of a local being declared inside inner blocks from shared parameter expressions.
  /// </summary>
  internal class BlockExpressionLocalsRewriter : ExpressionVisitor
  {
    private ParameterExpression[] _liftedLocals;

    public BlockExpressionLocalsRewriter(IEnumerable<ParameterExpression> liftedLocals)
    {
      _liftedLocals = liftedLocals.ToArray();
    }

    protected override Expression VisitBlock(BlockExpression node)
    {
      if (_liftedLocals.Length != 0)
      {
        return Expression.Block(node.Type, node.Variables.Where(p => !_liftedLocals.Contains(p)), Visit(node.Expressions));
      }

      return base.VisitBlock(node);
    }
  }
}
