using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public class RezolveContext
	{
		public static RezolveContext EmptyContext = new RezolveContext();

		public RezolveContext(string name = null, IRezolver dynamicRezolver = null, ILifetimeScopeRezolver scope = null)
		{

		}

		public RezolveContext()
			: this(null, null, null)
		{

		}

		public string Name { get; private set; }
		public IRezolver DynamicRezolver { get; private set; }
		public ILifetimeScopeRezolver Scope { get; private set; }
	}
}
