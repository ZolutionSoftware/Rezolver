﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public interface ISelfReferencingCovariant : ICovariant<ISelfReferencingCovariant>, ISelfReferencingCovariantBaseInterface
    {

    }
}
