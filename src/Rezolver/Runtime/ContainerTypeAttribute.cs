// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Runtime
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    internal class ContainerTypeAttribute : Attribute
    {
        // provides a way to shortcircuit the GetContainerType functionality - which is easy to blow (on internal code)
        // when using options types, as GetOption requires GetContainerType which, by default, also requires options!
        public Type Type { get; }

        public ContainerTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
