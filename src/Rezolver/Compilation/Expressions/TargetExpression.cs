// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;

namespace Rezolver.Compilation.Expressions
{
    /// <summary>
    /// An expression which represents an <see cref="ITarget" />, allowing a target with a particular <see cref="ITarget.DeclaredType"/> to be
    /// used in place of a traditional expression.
    /// </summary>
    /// <remarks>The <see cref="ExpressionTargetBuilder"/> uses this class extensively when translating expressions
    /// into targets and back again.  It's *highly* unlikely you'll need to use it in your code.
    ///
    /// It acts as a placeholder for targets until the point at which the expression compiler wants to build a
    /// complete expression tree for a target.</remarks>
    public class TargetExpression : Expression
    {
        private readonly ITarget _target;

        /// <summary>
        /// Gets the target whose expression will be subsituted for this TargetExpression in the final expression tree.
        /// </summary>
        public ITarget Target
        {
            get { return this._target; }
        }

        /// <summary>
        /// Gets the static type of the expression that this <see cref="Expression" /> represents.
        /// </summary>
        /// <remarks>Always returns the type referenced by the <see cref="ITarget.DeclaredType"/> property
        /// of <see cref="Target"/>.</remarks>
        public override Type Type
        {
            get { return this._target.DeclaredType; }
        }

        /// <summary>
        /// Gets the node type of this <see cref="Expression" />.
        /// </summary>
        /// <value>Always returns <see cref="ExpressionType.Extension"/>.</value>
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        /// <value><c>true</c> if this instance can reduce; otherwise, <c>false</c>.</value>
        /// <remarks>The implementation always returns <c>true</c>; although the <see cref="Reduce"/> methodd is not implemented.</remarks>
        public override bool CanReduce
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If CanReduce returns true, this should return a valid expression. This method can return another node which itself must be reduced.
        /// </summary>
        /// <exception cref="NotSupportedException">RezolveTargetExpression must be rewritten as a bona-fide expression before walking the expression tree for any other purpose</exception>
        public override Expression Reduce()
        {
            throw new NotSupportedException("RezolveTargetExpression must be rewritten as a bona-fide expression before walking the expression tree for any other purpose");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TargetExpression"/> class.
        /// </summary>
        /// <param name="target">The target.</param>
        public TargetExpression(ITarget target)
        {
            this._target = target;
        }
    }
}