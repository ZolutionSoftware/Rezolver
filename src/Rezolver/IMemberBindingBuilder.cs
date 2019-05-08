// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System.Reflection;

namespace Rezolver
{
    internal interface IMemberBindingBuilder
    {
        MemberInfo Member { get; }

        MemberBinding BuildBinding();
    }
}
