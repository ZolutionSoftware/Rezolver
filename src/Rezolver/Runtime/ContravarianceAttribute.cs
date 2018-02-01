// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Runtime
{
    /// <summary>
    /// Internal hack to prevent stack overflows when the <see cref="OptionsTargetContainerExtensions"/>
    /// options accessors recurse for other options lookups.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    internal class ContravarianceAttribute : Attribute
    {
        public bool Enable { get; }

        public ContravarianceAttribute(bool enable)
        {
            this.Enable = enable;
        }
    }
}
