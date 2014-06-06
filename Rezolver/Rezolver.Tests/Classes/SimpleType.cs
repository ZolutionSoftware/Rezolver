using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver.Tests.Classes
{
	public class SimpleType
	{
		private static int _instanceCount = 0;

		public static int InstannceCount
		{
			get
			{
				return _instanceCount;
			}
		}
		public SimpleType()
		{
			Interlocked.Increment(ref _instanceCount);
		}
	}
}
