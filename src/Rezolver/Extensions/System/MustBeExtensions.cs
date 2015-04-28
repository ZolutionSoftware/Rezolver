using System.Diagnostics;

namespace System
{
	internal static class MustBeExtensions
	{
		/// <summary>
		/// Helper method for argument validation - throws an ArgumentNullException if the passed object is null.
		/// 
		/// The <paramref name="paramName"/> parameter is used by the caller to indicate the name of the parameter whose
		/// argument was checked.
		/// </summary>
		/// <typeparam name="T">Type of the argument</typeparam>
		/// <param name="obj">The object.</param>
		/// <param name="paramName">Name of the parameter.</param>
		/// <exception cref="System.ArgumentNullException">If <paramref name="obj"/> is null.</exception>
		[DebuggerStepThrough]
		public static void MustNotBeNull<T>(this T obj, string paramName = null) where T : class
		{
			if (obj == null)
				throw new ArgumentNullException(paramName);
		}
	}
}
