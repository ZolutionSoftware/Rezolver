// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Rezolver.Targets;

namespace Rezolver
{
    /// <summary>
    /// Extensions for to simplify registering expressions in an <see cref="ITargetContainer"/>.
    /// </summary>
    public static partial class ExpressionTargetContainerExtensions
    {
        /// <summary>
        /// Registers the expression in the target container
        /// </summary>
        /// <param name="targetContainer">The target container in which the registration will be made.</param>
        /// <param name="expression">The expression to be registered.</param>
        /// <param name="declaredType">Optional.  The <see cref="ITarget.DeclaredType"/> of the target to be created,
        /// if different from the <see cref="Expression.Type"/> of the <paramref name="expression"/> (or its
        /// <see cref="LambdaExpression.Body"/> if the expression is a <see cref="LambdaExpression"/>).
        ///
        /// Will also override the type against which the expression will be registered if provided.</param>
        /// <param name="scopeBehaviour">Optional.  Controls how the object generated from the compiled expression will be
        /// tracked if the target is executed within an <see cref="IContainerScope" />.  The default is <see cref="ScopeBehaviour.Implicit" />.</param>
        /// <param name="scopePreference">Optional.  If <paramref name="scopeBehaviour"/> is not <see cref="ScopeBehaviour.None"/>, then this controls the
        /// type of scope in which the instance should be tracked.  Defaults to <see cref="ScopePreference.Current"/>.  <see cref="ScopePreference.Root"/>
        /// should be used if the result of the delegate is effectively a singleton.</param>
        public static void RegisterExpression(
            this ITargetContainer targetContainer, 
            Expression expression, 
            Type declaredType = null, 
            ScopeBehaviour scopeBehaviour = ScopeBehaviour.Implicit,
            ScopePreference scopePreference = ScopePreference.Current)
        {
            if (targetContainer == null)
            {
                throw new ArgumentNullException(nameof(targetContainer));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            ITarget toRegister = new ExpressionTarget(expression, declaredType);
            if (scopeBehaviour == ScopeBehaviour.Explicit)
            {
                toRegister = toRegister.Scoped();
            }
            else if (scopeBehaviour == ScopeBehaviour.None)
            {
                toRegister = toRegister.Unscoped();
            }

            targetContainer.Register(toRegister);
        }
    }
}
