﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class GenericWrapper<T>
    {
		public T Wrapped { get; }
		public GenericWrapper(T wrapped)
		{
			Wrapped = wrapped;
		}
    }
}
