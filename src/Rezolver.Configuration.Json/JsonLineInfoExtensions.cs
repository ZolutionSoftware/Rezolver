using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver.Configuration.Json
{
	internal static class JsonLineInfoExtensions
	{
		internal static string AtString(this IJsonLineInfo lineInfo)
		{
			if (lineInfo == null || !lineInfo.HasLineInfo())
				return "At unknown line";
			else
				return string.Format("At line {0}:{1}", lineInfo.LineNumber, lineInfo.LinePosition);
		}

		internal static string FormatMessageForThisLine(this IJsonLineInfo lineInfo, string format, params object[] args)
		{
			return string.Format(string.Format(format + " ({{0}})", args), lineInfo.AtString());
		}

		internal static IConfigurationLineInfo ToConfigurationLineInfo(this IJsonLineInfo start, IJsonLineInfo end = null)
		{
			return new JsonConfigurationLineInfo(start, end);
		}

		private class CapturedJsonLineInfo : IJsonLineInfo
		{
			private readonly bool _hasLineInfo;

			public bool HasLineInfo()
			{
				return _hasLineInfo;
			}
			public int LineNumber
			{
				get;
				private set;
			}

			public int LinePosition
			{
				get;
				private set;
			}
			public CapturedJsonLineInfo(IJsonLineInfo source)
			{
				_hasLineInfo = source.HasLineInfo();
				if (_hasLineInfo)
				{
					LineNumber = source.LineNumber;
					LinePosition = source.LinePosition;
				}
			}
		}

		internal static IJsonLineInfo Capture(this IJsonLineInfo source)
		{
			return new CapturedJsonLineInfo(source);
		}
	}
}
