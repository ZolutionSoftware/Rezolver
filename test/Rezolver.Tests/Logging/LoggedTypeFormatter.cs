using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Logging
{
	/// <summary>
	/// Target type for auto-registration is derived from the LoggingFormatter{T} type argument
	/// </summary>
	/// <seealso cref="Rezolver.Logging.LoggingFormatter{Rezolver.Tests.Logging.LoggedType}" />
	[LoggingFormatter]
	public class LoggedTypeFormatter : LoggingFormatter<LoggedType>
	{
		public override string Format(LoggedType obj, string format = null, LoggingFormatterCollection formatters = null)
		{
			return Expected(obj, format, formatters);
		}

		public static string Expected(LoggedType obj, string format = null, LoggingFormatterCollection formatters = null)
		{
			return $"LoggedTypeFormatter { string.Join(",", Enumerable.Range(0, obj.IntValue).Select(i => obj.StringValue)) }";
		}
	}
}
