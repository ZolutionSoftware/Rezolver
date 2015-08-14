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

		/// <summary>
		/// Helper method for argument validation - throws an ArgumentException if the predicate returns true for the passed
		/// value.
		/// </summary>
		/// <typeparam name="T">Type of the argument being checked</typeparam>
		/// <param name="obj">The object to be validated</param>
		/// <param name="predicate">Predicate providing the test to be performed.  If this returns true then the exception will be thrown.</param>
		/// <param name="message">Optional - message to be included in the ArgumentException that is raised.</param>
		/// <param name="paramName">Optional (but desirable) - name of the parameter to the function requesting validation.</param>
		[DebuggerStepThrough]
		public static void MustNot<T>(this T obj, Func<T, bool> predicate, string message = null, string paramName = null)
		{
			predicate.MustNotBeNull(nameof(predicate));
			if (predicate(obj))
				throw new ArgumentException(message, paramName);
		}
	}
}
