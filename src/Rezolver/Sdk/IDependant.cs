// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Interface for an object that is dependent upon other objects
    /// </summary>
    public interface IDependant
    {
        /// <summary>
        /// Contains all dependency metadata for this object.
        /// </summary>
        IEnumerable<DependencyMetadata> Dependencies { get; }
    }
}