using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests
{
    internal static class ContainerScopeResolveExtensions
    {

		public static TResult Resolve<TResult>(this IContainerScope scope, Guid targetId, IResolveContext context, Func<IResolveContext, object> factory, ScopeBehaviour behaviour)
		{
			if (scope == null) throw new ArgumentNullException(nameof(scope));
			return (TResult)scope.Resolve(context, targetId, factory, behaviour);
		}
	}
}
