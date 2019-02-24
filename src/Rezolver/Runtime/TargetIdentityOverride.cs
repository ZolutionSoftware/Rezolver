// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Runtime
{
    internal class TargetIdentityOverride
    {
        public TargetIdentityOverride(int targetId)
        {
            TargetId = targetId;
        }

        public int TargetId { get; }

        public static implicit operator int(TargetIdentityOverride tio)
        {
            return (tio ?? throw new ArgumentNullException(nameof(tio))).TargetId;
        }
    }
}
