﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal abstract class GenericBase<T> : IGeneric<T>
	{
		public T Value
		{
			get; protected set;
		}

		public abstract void Foo();
	}
}
