using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Logging
{
    public class LoggedTypeExtra : LoggedType, ILoggedType_Decimal
	{
		public decimal DecimalValue { get; set; }
    }
}
