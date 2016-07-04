// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Rezolver
{
    /// <summary>
    /// Rewrites the first NewExpression found for a particular type into a <see cref="MemberInitExpression"/>
    /// with the given bindings.  If no type is specified on construction, then the first NewExpression that
    /// is encountered will be used.
    /// </summary>
    internal class NewExpressionMemberInitRewriter : ExpressionVisitor
    {
        private readonly Type _ctorType;
        private readonly IEnumerable<MemberBinding> _newBindings;
        private bool _found;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctorType"></param>
        /// <param name="newBindings"></param>
        public NewExpressionMemberInitRewriter(Type ctorType, IEnumerable<MemberBinding> newBindings)
        {
            _ctorType = ctorType;
            _newBindings = newBindings;
        }
        protected override Expression VisitNew(NewExpression node)
        {
            if (!_found)
            {
                if (node.Type == (_ctorType ?? node.Type))
                {
                    //we've found the constructor call that we're after
                    _found = true;
                    return Expression.MemberInit(node, _newBindings);
                }
            }
            return base.VisitNew(node);
        }
    }
}
