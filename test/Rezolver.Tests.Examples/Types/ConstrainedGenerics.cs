// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
    public interface IGeneric<T>
    {
        
    }

    public class GenericAny<T> : IGeneric<T>
    {

    }

    public class GenericAnyIMyService<T> : IGeneric<T>
        where T: IMyService
    {

    }

    public class GenericAnyMyService1<T> : IGeneric<T>
        where T: MyService1
    {

    }

    /// <summary>
    /// Note - used in the per-service 'best match only' example
    /// </summary>
    public class GenericMyService2 : IGeneric<MyService2>
    {

    }
    // </example>
}
