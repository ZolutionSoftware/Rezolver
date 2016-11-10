using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
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

		protected virtual string FormatWithStackTraceOnly(TException exception, string format = Format_WithStackTrace, LoggingFormatterCollection formatters = null)
		{
			return string.Format("Exception of type {0}, message: {1}{2}Stack Trace: {3}", exception.GetType(), exception.Message, Environment.NewLine, exception.StackTrace);
		}

		protected virtual string FormatDefault(TException exception, LoggingFormatterCollection formatters = null)
		{
			return string.Format("Exception of type {0}, message: {1}", exception.GetType(), exception.Message);
		}
	}
}

