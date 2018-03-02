// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal class DefaultTargetContainerTypeResolver : ITargetContainerTypeResolver
    {
        internal static DefaultTargetContainerTypeResolver Instance { get; } = new DefaultTargetContainerTypeResolver();

        private DefaultTargetContainerTypeResolver() { }

        public Type GetContainerType(Type serviceType)
        {
            return serviceType;
        }
    }
}
