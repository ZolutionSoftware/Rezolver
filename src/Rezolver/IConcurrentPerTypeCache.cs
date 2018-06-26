// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;

namespace Rezolver
{
    internal interface IConcurrentPerTypeCache<TValue>
    {
        TValue Get(Type type, Func<TValue> valueFactory);
    }
}