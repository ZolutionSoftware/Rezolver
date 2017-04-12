using System;
using System.Collections.Generic;
using System.Text;

namespace Rezolver.Tests.Types
{
    public class RequiresInstanceCountingType
    {
        public InstanceCountingType Instance { get; }
        public RequiresInstanceCountingType(InstanceCountingType instance)
        {
            Instance = instance;
        }
    }
}
