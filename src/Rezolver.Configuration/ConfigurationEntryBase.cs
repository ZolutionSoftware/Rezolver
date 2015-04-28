using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	/// <summary>
	/// suggested starting point for implementing your own configuration entries.
	/// </summary>
	public class ConfigurationEntryBase : IConfigurationEntry
	{
		public ConfigurationEntryType Type
		{
			get;
			private set;
		}

		public int? StartLineNo
		{
			get;
			private set;
		}

		public int? StartLinePos
		{
			get;
			private set;
		}

		public int? EndLineNo
		{
			get;
			private set;
		}

		public int? EndLinePos
		{
			get;
			private set;
		}

		protected ConfigurationEntryBase(ConfigurationEntryType type, IConfigurationLineInfo lineInfo)
		{
			if (type == ConfigurationEntryType.Extension && !(this is IConfigurationExtensionEntry))
				throw new ArgumentException("If type is Extension then this instance must implement IConfigurationExtensionEntry");

			Type = type;
			if(lineInfo != null)
			{
				StartLineNo = lineInfo.StartLineNo;
				StartLinePos = lineInfo.StartLinePos;
				EndLineNo = lineInfo.EndLineNo;
				EndLinePos = lineInfo.EndLinePos;
			}
		}
	}
}
