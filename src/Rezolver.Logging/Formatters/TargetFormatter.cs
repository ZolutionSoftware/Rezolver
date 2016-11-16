using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class TargetFormatter : ObjectFormatter<ITarget>
    {
		public override string Format(ITarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{0} with DeclaredType {1} (Is Fallback? {0})", obj.GetType(), obj.DeclaredType, obj.UseFallback);
		}
	}
}
