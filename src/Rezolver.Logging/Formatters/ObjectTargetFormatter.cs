using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class ObjectTargetFormatter : ObjectFormatter<ObjectTarget>
    {
		public override string Format(ObjectTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{{ {0} ({1}) }}", obj.Value, obj.DeclaredType);
		}
	}
}
