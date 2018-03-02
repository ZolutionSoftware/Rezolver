// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver
{

    [Flags]
    internal enum Contravariance
    {
        None = 0,
        Bases = 1,
        Interfaces = 2,
        BasesAndInterfaces = Bases | Interfaces,
        Derived = 4
    }
}
