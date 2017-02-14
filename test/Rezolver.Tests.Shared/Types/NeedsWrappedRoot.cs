using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Types
{
	/// <summary>
	/// This class is used as an argument to a GenericWrapper&lt;&gt; resolve call which,
	/// in turn, then requests a GenericWrapper&lt;Root&gt; to be resolved - compilation
	/// must allow the same target to appear in the compile stack twice for different concrete
	/// types to allow this to happen.
	/// </summary>
	public class NeedsWrappedRoot
    {
		public GenericWrapper<Root> WrappedRoot { get; }

		public NeedsWrappedRoot(GenericWrapper<Root> wrappedRoot)
		{
			WrappedRoot = wrappedRoot;
		}

		public override string ToString()
		{
			return $"NeedsWrappedRoot with { WrappedRoot.ToString() }";
		}
	}
}
