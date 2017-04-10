using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
	public class TestCompiledTarget : ICompiledTarget
	{
		private object _obj;

        public ITarget SourceTarget => null;

		public TestCompiledTarget(object obj = null)
		{
			_obj = obj;
		}

		public object GetObject(IResolveContext context)
		{
			return _obj;
		}
	}
}
