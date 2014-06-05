using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	 internal static class MustBeExtensions
	 {
		  public static void MustNotBeNull<T>(this T obj, string paramName = null) where T : class
		  {
				if (obj == null)
					 throw new ArgumentNullException(paramName);
		  }
	 }
}
