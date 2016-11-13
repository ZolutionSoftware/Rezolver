using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Logging
{
	public class LoggedType : ILoggedType_Int, ILoggedType_String
	{
		static LoggedType()
		{

		}

		public int IntValue { get; set; }
		public string StringValue { get; set; }
	}
}
