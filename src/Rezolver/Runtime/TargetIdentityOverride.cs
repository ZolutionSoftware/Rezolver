// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Runtime
{
    internal class TargetIdentityOverride
    {
        public TargetIdentityOverride(Guid targetId)
        {
            this.TargetId = targetId;
        }

        public Guid TargetId { get; }

        public static implicit operator Guid(TargetIdentityOverride tio)
        {
            return (tio ?? throw new ArgumentNullException(nameof(tio))).TargetId;
        }
    }
}
