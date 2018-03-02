// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Standard extensions for the <see cref="IContainerScope"/>
    /// </summary>
    public static class ContainerScopeExtensions
    {
        /// <summary>
        /// Gets the root-most scope for the scope on which this method is called.
        ///
        /// Note that the result is calculated by walking up the tree of
        /// <see cref="IContainerScope.Parent"/> scopes until one is reached
        /// that does not have a parent.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="scope"/> is null</exception>
        public static IContainerScope GetRootScope(this IContainerScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            while (scope.Parent != null) { scope = scope.Parent; }
            return scope;
        }
    }
}
