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
  /// Rewrites the first NewExpression found for a particular type into a <see cref="MemberInitExpression"/>
  /// with the given bindings.  If no type is specified on construction, then the first NewExpression that
  /// is encountered will be used.
  /// </summary>
  internal class NewExpressionMemberInitRewriter : ExpressionVisitor
  {
    private readonly Type _ctorType;
    private readonly IEnumerable<System.Linq.Expressions.MemberBinding> _newBindings;
    private bool _found;

    /// <summary>
    ///
    /// </summary>
    /// <param name="ctorType"></param>
    /// <param name="newBindings"></param>
    public NewExpressionMemberInitRewriter(Type ctorType, IEnumerable<System.Linq.Expressions.MemberBinding> newBindings)
    {
      this._ctorType = ctorType;
      this._newBindings = newBindings;
    }

    protected override Expression VisitNew(NewExpression node)
    {
      if (!this._found)
      {
        if (node.Type == (this._ctorType ?? node.Type))
        {
          // we've found the constructor call that we're after
          this._found = true;
          return Expression.MemberInit(node, this._newBindings);
        }
      }

      return base.VisitNew(node);
    }
  }
}
