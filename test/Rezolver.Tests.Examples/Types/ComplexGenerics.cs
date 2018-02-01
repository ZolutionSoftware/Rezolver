// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //unlike the other files, we'll roll up a bunch of types in one
    //<example>
    //these types are just used as easy-to-read type arguments
    public class T1 { }
    public class T2 { }
    public class T3 { }

    public class BaseGeneric<T, U, V> { }

    public class MidGeneric<T, U, V> : BaseGeneric<V, U, T> { }

    public class FinalGeneric<T, U, V> : MidGeneric<U, T, V> { }

    // This type 'closes' over one type parameter
    public class ClosingGeneric<T, U> : FinalGeneric<T, U, string> { }
    //</example>
}
