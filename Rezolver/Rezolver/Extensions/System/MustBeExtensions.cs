using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

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
