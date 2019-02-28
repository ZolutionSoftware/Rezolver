// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Describes different ways in which objeects interact with scopes.
    /// </summary>
    /// <remarks>Note: this enum *might* be replaced with an abstraction in the future.  If so,
    /// it will not alter how regatrations are performed, but it will affect any low-level code
    /// which uses this enum directly.</remarks>
    public enum ScopeBehaviour
    {
        /// <summary>
        /// Implicitly scoped objects are only added to the scope
        /// for the purposes of disposing when the scope is disposed
        /// </summary>
        Implicit = 0,
        /// <summary>
        /// Explicitly scoped objects act like singletons in the current scope, regardless of
        /// whether they are disposable or not.
        /// </summary>
        Explicit = 1,
        /// <summary>
        /// The object will not be tracked in any scope, regardless of whether there is one active,
        /// or whether the object is disposable.
        /// </summary>
        None
    }

    /// <summary>
    /// Specifies the scope that the object produced by a target should be tracked within.
    /// </summary>
    public enum ScopePreference
    {
        /// <summary>
        /// The object will be tracked within the scope already set on the <see cref="ResolveContext"/>
        /// </summary>
        Current = 0,
        /// <summary>
        /// The object (and all its dependants) will be tracked within the root scope of the current scope
        /// set on the <see cref="ResolveContext"/>
        /// </summary>
        Root
    }

    
}
