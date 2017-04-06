using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
    // <example>
	public class DisposableType : IDisposable
	{
		public bool Disposed { get; private set; }
		public void Dispose()
		{
			if (Disposed) throw new ObjectDisposedException(nameof(DisposableType));
			Disposed = true;
		}
	}
    // </example>
}
