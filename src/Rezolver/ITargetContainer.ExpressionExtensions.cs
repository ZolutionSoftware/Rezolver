// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
		public static void RegisterExpression(this ITargetContainer targetContainer, Expression expression, Type declaredType = null)
		{
            if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
            if (expression == null) throw new ArgumentNullException(nameof(expression));

			targetContainer.Register(new ExpressionTarget(expression), declaredType);
		}
	}
}
