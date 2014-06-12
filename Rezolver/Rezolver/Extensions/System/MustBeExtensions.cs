using System.Diagnostics;

namespace System
{
	internal static class MustBeExtensions
	{
		[DebuggerStepThrough]
		public static void MustNotBeNull<T>(this T obj, string paramName = null) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException(paramName);
		}
	}
}
