// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

namespace Rezolver.Tests.Examples.Types
{
    //<example>
    public interface IDataFormatter<TData>
    {
        string FormatData(TData data);
    }
    //</example>
}