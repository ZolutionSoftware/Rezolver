﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class Covariant<T> : ICovariant<T>
    {
        public T Out()
        {
            throw new NotImplementedException();
        }
    }
}
