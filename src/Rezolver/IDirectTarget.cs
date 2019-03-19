// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver
{
    internal interface IDirectTarget : ITarget
    {
        /// <summary>
        /// Gets the object
        /// </summary>
        /// <returns></returns>
        object GetValue();
    }
}
