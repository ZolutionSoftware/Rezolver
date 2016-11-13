using Rezolver.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Logging
{
	[LoggingFormatter(typeof(ILoggedType_String), typeof(ILoggedType_Int), typeof(ILoggedType_Decimal))]
    public class LoggedTypeInterfaceFormatter : LoggingFormatter
    {
		public override string Format(object obj, string format = null, LoggingFormatterCollection formatters = null)
		{
			ILoggedType_String lStr = obj as ILoggedType_String;
			List<string> strs = new List<string>();
			if (lStr != null)
				strs.Add(Expected(lStr, format, formatters));

			ILoggedType_Decimal lDec = obj as ILoggedType_Decimal;

			if (lDec != null)
				strs.Add(Expected(lDec, format, formatters));

			ILoggedType_Int lInt = obj as ILoggedType_Int;

			if (lInt != null)
				strs.Add(Expected(lInt, format, formatters));

			if (strs.Count != 0)
				return string.Join(", ", strs);

			throw new NotSupportedException("The object is not supported");
		}

		public static string Expected(ILoggedType_String lStr, string format = null, LoggingFormatterCollection formatters = null)
		{
			switch (format)
			{
				case "alt":
					return $"Alternative via string interface: { lStr.StringValue }";
				default:
					return $"Via string interface: { lStr.StringValue }";
			}
		}

		public static string Expected(ILoggedType_Decimal lDec, string format = null, LoggingFormatterCollection formatters = null)
		{
			switch (format)
			{
				case "alt":
					return $"Alternative via decimal interface: { lDec.DecimalValue}";
				default:
					return $"Via decimal interface: { lDec.DecimalValue }";
			}
		}

		public static string Expected(ILoggedType_Int lInt, string format = null, LoggingFormatterCollection formatters = null)
		{
			switch (format)
			{
				case "alt":
					return $"Alternative via integer interface: { lInt.IntValue}";
				default:
					return $"Via integer interface: { lInt.IntValue }";
			}
		}
	}
}
