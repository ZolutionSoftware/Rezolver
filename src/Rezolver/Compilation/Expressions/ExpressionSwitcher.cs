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
    /// Replaces expressions within an expression tree with others.
    ///
    /// Expressions are compared by reference - so the class is most useful for replacing
    /// expressions like parameters or constants.
    ///
    /// When a replacement is deemed necessary, the visitor doesn't visit the replacement, it
    /// simply returns it in place of visiting the original.
    /// </summary>
    internal class ExpressionSwitcher : ExpressionVisitor
    {
        private readonly ExpressionReplacement[] _replacements;

        public ExpressionSwitcher(ExpressionReplacement[] replacements)
        {
            this._replacements = replacements;
        }

        public override Expression Visit(Expression node)
        {
            for(var f = 0; f<_replacements.Length; f++)
            {
                if (_replacements[f].Original == node)
                    return base.Visit(_replacements[f].Replacement);
            }

            return base.Visit(node);
        }
    }
}
