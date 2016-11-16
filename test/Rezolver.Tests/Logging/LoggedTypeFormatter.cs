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
	/// <seealso cref="Rezolver.Logging.ObjectFormatter{Rezolver.Tests.Logging.LoggedType}" />
	[ObjectFormatter]
	public class LoggedTypeFormatter : ObjectFormatter<LoggedType>
	{
		public override string Format(LoggedType obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return Expected(obj, format, formatters);
		}

		public static string Expected(LoggedType obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return $"LoggedTypeFormatter { string.Join(",", Enumerable.Range(0, obj.IntValue).Select(i => obj.StringValue)) }";
		}
	}
}
