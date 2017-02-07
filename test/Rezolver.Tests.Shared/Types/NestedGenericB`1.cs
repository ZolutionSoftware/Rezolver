﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class NestedGenericB<T> : INestedGenericB<T>
	{
		public IEnumerable<T> Value
		{
			get; private set;
		}

		public void Foo()
		{
			throw new NotImplementedException();
		}
	}
}