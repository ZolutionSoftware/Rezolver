﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Decorated2 : IDecorated
	{
		public string DoSomething()
		{
			return "Goodbye";
		}
	}
}