using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	/// <summary>
	/// Base class for Exception formatters which supplies the logic for handling the standard format arguments, [none], 'inner' and 'stacktrace'
	/// </summary>
	/// <typeparam name="TException">The type of the t exception.</typeparam>
	/// <seealso cref="Rezolver.Logging.LoggingFormatter{TException}" />
	/// <remarks>
	/// The <see cref="DefaultExceptionFormatter"/>, which will handle any exception type by default, derives from this, setting the <typeparamref name="TException"/>
	/// to <see cref="Exception"/>.  If you want to customise the formatting for a specific exception type, then you can derive from this, passing the exception
	/// type in <typeparamref name="TException"/> - even if the DefaultExceptionFormatter is registered in the same <see cref="LoggingFormatterCollection"/>
	/// </remarks>
	public class ExceptionFormatter<TException> : LoggingFormatter<TException>
		where TException : Exception
    {
		public const string Format_WithInnerException = "inner";
		public const string Format_WithStackTrace = "stacktrace";

		public override string Format(TException obj, string format = null, LoggingFormatterCollection formatters = null)
		{
			if(Format_WithInnerException.Equals(format, StringComparison.OrdinalIgnoreCase))
			{
				return FormatWithInnerException(obj, format, formatters: formatters);
			}
			else if(Format_WithStackTrace.Equals(format, StringComparison.OrdinalIgnoreCase))
			{
				return FormatWithStackTraceOnly(obj, format, formatters: formatters);
			}
			else
			{
				return FormatDefault(obj, formatters: formatters);
			}
		}

		/// <summary>
		/// Called by <see cref="Format(TException, string, LoggingFormatterCollection)"/> when the format argument is set to <see cref="Format_WithInnerException"/>.
		/// 
		/// Override this to customise how exceptions of the type <typeparamref name="TException"/> are reported with their stack traces and inner exceptions.
		/// 
		/// The base implementation uses the <see cref="FormatWithStackTraceOnly(TException, string, LoggingFormatterCollection)"/> function first, and then
		/// will either use the <paramref name="formatters"/> to include the formatted inner exception, or will recurse if no <paramref name="formatters"/> are
		/// passed.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="format">The format.</param>
		/// <param name="formatters">The formatters to be used for inner exceptions or any other data, if applicable.  This will always be passed if
		/// this formatter is invoked by the <see cref="LoggingFormatterCollection.Format(object, string, IFormatProvider)"/> memthod or any of its overloads.</param>
		protected virtual string FormatWithInnerException(TException exception, string format = Format_WithInnerException, LoggingFormatterCollection formatters = null)
		{
			if (exception.InnerException != null)
			{
				return string.Join(Environment.NewLine, new[] { FormatWithStackTraceOnly(exception, formatters: formatters),
					"Inner Exception:",
					formatters != null ? formatters.Format(exception.InnerException) : FormatWithInnerException(exception, formatters: formatters) });
			}
			else
				return FormatWithStackTraceOnly(exception, formatters: formatters);
		}

		/// <summary>
		/// Called by <see cref="Format(TException, string, LoggingFormatterCollection)"/> when the format argument is set to <see cref="Format_WithStackTrace"/>.
		/// It is also called by <see cref="FormatWithInnerException(TException, string, LoggingFormatterCollection)"/> to get the first part of the exception
		/// text before the inner exception report is output.
		/// 
		/// Override this to customise how exceptions of the type <typeparamref name="TException"/> are reported with their stack traces.
		/// 
		/// The base implementation first calls <see cref="FormatDefault(TException, LoggingFormatterCollection)"/> to get the first part of the output string.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="format">The format.</param>
		/// <param name="formatters">The formatters.</param>
		protected virtual string FormatWithStackTraceOnly(TException exception, string format = Format_WithStackTrace, LoggingFormatterCollection formatters = null)
		{
			return string.Format("{0}{1}Stack Trace: {2}", FormatDefault(exception, formatters), Environment.NewLine, exception.StackTrace);
		}

		/// <summary>
		/// Formats the exception using the default format - which includes the exception type and message.
		/// 
		/// This method is used when no explicit format is provided to <see cref="Format(TException, string, LoggingFormatterCollection)"/>, or if the format 
		/// string is unrecognised.
		/// 
		/// It is also used directly by the default implementation of <see cref="FormatWithStackTraceOnly(TException, string, LoggingFormatterCollection)"/>
		/// and indirectly by <see cref="FormatWithInnerException(TException, string, LoggingFormatterCollection)"/>.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="formatters">The formatters.</param>
		/// <returns>System.String.</returns>
		protected virtual string FormatDefault(TException exception, LoggingFormatterCollection formatters = null)
		{
			return string.Format("Exception of type {0}, message: {1}", exception.GetType(), exception.Message);
		}
	}

	[LoggingFormatter]
	public class DefaultExceptionFormatter : ExceptionFormatter<Exception>
	{
		public DefaultExceptionFormatter() { }
	}
}