using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public interface IConfiguration
	{
		string FileName { get; }
		IEnumerable<IConfigurationEntry> Entries { get; }
	}
}
