using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Compilation.Expressions.Types
{
	public class ScopedType : IDisposable
	{
		public bool Disposed { get; private set; }
		public void Dispose()
		{
			if (Disposed) throw new InvalidOperationException("Object already disposed");
			Disposed = true;
		}
	}
}
