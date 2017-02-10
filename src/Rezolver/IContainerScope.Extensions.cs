using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
    public static class ContainerScopeExtensions
    {
		public static IContainerScope GetRootScope(this IContainerScope scope)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			while (scope.Parent != null) { scope = scope.Parent; }
			return scope;
		}
	}
}
