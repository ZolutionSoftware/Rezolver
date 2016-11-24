using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
	public class GenericConstructorTargetFormatter : ObjectFormatter<GenericConstructorTarget>
	{
		public override string Format(GenericConstructorTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{{ New {0} }}", obj.DeclaredType);
		}
	}
}
