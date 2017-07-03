using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver
{
	internal class StubContainer : IContainer
	{
		internal static StubContainer Instance { get; } = new StubContainer();

		private StubContainer() { }
		public bool CanResolve(IResolveContext context)
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}

		public IContainerScope CreateScope()
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}

		public ICompiledTarget GetCompiledTarget(IResolveContext context)
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}

		public object GetService(Type serviceType)
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}

		public object Resolve(IResolveContext context)
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}

		public bool TryResolve(IResolveContext context, out object result)
		{
			throw new InvalidOperationException("The ResolveContext has no Container set");
		}
	}
}
