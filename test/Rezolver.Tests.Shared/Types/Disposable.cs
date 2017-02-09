using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	internal class Disposable : IDisposable
	{
		public bool Disposed { get; private set; } = false;
		public int DisposedCount { get; private set; } = 0;
		public void Dispose()
		{
			Disposed = true;
			++DisposedCount;
		}
	}
}
