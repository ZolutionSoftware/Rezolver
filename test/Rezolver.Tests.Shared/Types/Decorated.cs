﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Decorated : IDecorated
	{
		public string DoSomething()
		{
			return "Hello";
		}
	}
}
