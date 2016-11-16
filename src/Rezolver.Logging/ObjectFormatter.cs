using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	/// <summary>
	/// An object used when formatting messages for <see cref="TrackedCall"/> instances.
	/// </summary>
	public class ObjectFormatter
	{
		/// <summary>
		/// Gets a string representing the value of an object for the purposes of displaying in messages in <see cref="TrackedCall" /> instances.
		/// If the formatter is not able to produce a valid string for the object, then it should return null.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="format">Optional.  Implementation-specific format string which controls the way the <paramref name="obj" /> is output to string.</param>
		/// <param name="formatters">Optional.  The formatters collection to be used for any composite formatting required by this formatter.  This is
		/// typically the collection to which this formatter belongs; therefore the formatter must be careful not to recurse indirectly through
		/// this collection.  This parameter is always supplied when a <see cref="ObjectFormatter"/> is invoked by a <see cref="ObjectFormatterCollection"/>.</param>
		/// <returns>A string for the <paramref name="obj" /></returns>
		/// <remarks>The base implementation returns "null" for a null reference.
		/// For a non-null reference, the function
		/// either returns the result of the object's <see cref="object.ToString" /> function, or, if the object is
		/// <see cref="IFormattable" />, the result of its <see cref="IFormattable.ToString(string, IFormatProvider)" /> function,
		/// passing the <paramref name="format" /> argument through as provided.
		/// Note that there is no way to pass through a custom format provider to an IFormattable, because the
		/// <see cref="ObjectFormatterCollection" /> is itself an IFormatProvider, and so there's no way for it
		/// to pass another one through.  If you need to format an object in a particular way which also relies on a
		/// particular format provider, then you should format it manually first, then pass the formatted string.</remarks>
		public virtual string Format(object obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			if (obj != null)
				return obj is IFormattable ? ((IFormattable)obj).ToString(format, null) : obj.ToString();
			
			return "[null]";
		}
	}
}
