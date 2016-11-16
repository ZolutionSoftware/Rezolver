using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class DefaultTargetFormatter : ObjectFormatter<DefaultTarget>
    {
		public override string Format(DefaultTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{{ Default({0}) }}", obj.DeclaredType);
		}
	}
}
