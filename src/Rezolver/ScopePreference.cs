// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver
{
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
        /// The object (and all its dependants) will be tracked within the <see cref="ContainerScope2.Root"/> scope of the current scope
        /// set on the <see cref="ResolveContext"/>
        /// </summary>
        Root
    }
}
