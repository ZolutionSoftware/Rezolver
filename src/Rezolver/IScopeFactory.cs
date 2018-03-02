// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Represents an object from which a scope can be created
    /// </summary>
    public interface IScopeFactory
    {
        /// <summary>
        /// Creates a new scope.  If the implementing object is also a scope, then the new scope must be
        /// created as a child scope of that scope.
        /// </summary>
        IContainerScope CreateScope();
    }
}
