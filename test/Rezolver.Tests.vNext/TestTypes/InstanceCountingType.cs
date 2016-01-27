using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Tests.vNext.TestTypes
{
	public class InstanceCountingType
	{
		private static int _instanceCount = 0;

		public static int InstanceCount
		{
			get
			{
				return _instanceCount;
			}
		}
		public InstanceCountingType()
		{
			Interlocked.Increment(ref _instanceCount);
		}
	}
}
