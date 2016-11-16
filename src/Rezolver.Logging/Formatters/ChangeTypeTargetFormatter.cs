using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class ChangeTypeTargetFormatter : ObjectFormatter<ChangeTypeTarget>
    {
		public override string Format(ChangeTypeTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{{ {0} as {1} }}", obj.InnerTarget, obj.DeclaredType);
		}
	}
}
