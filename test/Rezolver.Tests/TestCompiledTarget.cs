﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestCompiledTarget : ICompiledTarget
	{
		private object _obj;
		public TestCompiledTarget(object obj = null)
		{
			_obj = obj;
		}

		public object GetObject(ResolveContext context)
		{
			return _obj;
		}
	}
}