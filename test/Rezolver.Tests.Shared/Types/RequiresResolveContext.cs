using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresResolveContext
    {
		public IResolveContext Context { get; }
		public RequiresResolveContext(IResolveContext context)
		{
			Context = context;
		}
    }
}
