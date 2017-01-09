﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class DecoratorTargetFormatter : ObjectFormatter<DecoratingTargetContainer>
    {
		public override string Format(DecoratingTargetContainer obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{{ {0} Decorating {1} }}", obj.DecoratorType, obj.DecoratedType);
		}
	}
}
