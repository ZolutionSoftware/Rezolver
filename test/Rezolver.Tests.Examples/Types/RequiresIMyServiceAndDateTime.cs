using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Tests.Examples.Types
{
	//<example>
	public class RequiresIMyServiceAndDateTime
	{
		public DateTime StartDate { get; }
		public IMyService Service { get; }

		public RequiresIMyServiceAndDateTime(IMyService service)
			: this(service, DateTime.UtcNow)
		{

		}

		public RequiresIMyServiceAndDateTime(IMyService service, DateTime startDate)
		{
			Service = service;
			StartDate = startDate;
		}
	}
	//</example>
}
