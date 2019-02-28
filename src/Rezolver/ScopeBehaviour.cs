// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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

    
}
