using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class LoggingFormatterAttribute : Attribute
    {
		public Type[] AssociatedTypes { get; }

		public LoggingFormatterAttribute(params Type[] associatedTypes)
		{
			AssociatedTypes = associatedTypes;
		}
    }
}
