using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Runtime
{
    internal class TargetIdentityOverride
    {
        public TargetIdentityOverride(Guid targetId)
        {
            TargetId = targetId;
        }

        public Guid TargetId { get; }

        public static implicit operator Guid(TargetIdentityOverride tio)
        {
            return (tio ?? throw new ArgumentNullException(nameof(tio))).TargetId;
        }
    }
}
