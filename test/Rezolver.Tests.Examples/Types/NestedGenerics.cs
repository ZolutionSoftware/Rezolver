// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public interface IGenericService<T>
    {

    }
    public class GenericEnumerableService<T>
        : IGenericService<IEnumerable<T>>
    {

    }

    public class GenericEnumerableNullableService<T>
        : IGenericService<IEnumerable<Nullable<T>>>
        where T : struct
    {

    }
    //</example>
}
