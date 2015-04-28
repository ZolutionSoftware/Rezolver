using System.Threading;

namespace Rezolver.Tests.Classes
{
	public class SimpleType
	{
		private static int _instanceCount = 0;

		public static int InstanceCount
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
