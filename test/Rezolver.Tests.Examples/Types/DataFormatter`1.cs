using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class DataFormatter<TData> : IDataFormatter<TData>
	{
		public string FormatData(TData data)
		{
			return data.ToString();
		}
	}
	//</example>
}
