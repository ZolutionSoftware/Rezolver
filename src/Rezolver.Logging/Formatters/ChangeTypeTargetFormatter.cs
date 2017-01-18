// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
