using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public interface IConfigurationEntry : IConfigurationLineInfo
	{
		ConfigurationEntryType Type { get; }
	}
}
