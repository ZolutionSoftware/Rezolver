using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
    public class RequiresResolveContext
    {
		public ResolveContext Context { get; }
		public RequiresResolveContext(ResolveContext context)
		{
			Context = context;
		}
    }
}
