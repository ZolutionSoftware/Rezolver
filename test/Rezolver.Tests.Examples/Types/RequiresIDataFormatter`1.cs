using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class RequiresIDataFormatter<T>
	{
		public IDataFormatter<T> Formatter { get; }

		public RequiresIDataFormatter(IDataFormatter<T> formatter)
		{
			Formatter = formatter;
		}
	}
	//</example>
}
