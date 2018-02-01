// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,
        AllowMultiple = false,
        Inherited = false)]
    public class InjectAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type to be resolved.
        /// from the container for the associated member.
        /// 
        /// If null, then the member's type will be used.
        /// 
        /// If not null, then, obviously, it must be compatible!
        /// </summary>
        public Type Type { get; }

        public InjectAttribute(Type type = null)
        {
            Type = type;
        }
    }
    //</example>
}
