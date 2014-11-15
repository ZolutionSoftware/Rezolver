using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver.Configuration
{
	public interface IConfigurationLineInfo
	{
		int? StartLineNo { get; }
		int? StartLinePos { get; }
		int? EndLineNo { get; }
		int? EndLinePos { get; }
	}
}
