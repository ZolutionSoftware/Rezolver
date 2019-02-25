﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    public interface IActivator
    {
        object Activate(Activation activation, Func<IResolveContext, object> factory, IResolveContext context);
    }
}
