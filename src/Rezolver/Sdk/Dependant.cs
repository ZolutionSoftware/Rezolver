﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Collections.Generic;

namespace Rezolver.Sdk
{
    /// <summary>
    /// Simple base class for implementations of <see cref="IDependant"/> - the <see cref="IDependant.Dependencies"/>
    /// property is implemented explictly.
    /// </summary>
    public class Dependant : IMutableDependant
    {
        private DependencyMetadataCollection Dependencies { get; } = new DependencyMetadataCollection();

        DependencyMetadataCollection IMutableDependant.Dependencies => Dependencies;

        IEnumerable<DependencyMetadata> IDependant.Dependencies => Dependencies;
    }
}
