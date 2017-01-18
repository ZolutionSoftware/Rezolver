// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
    public class SingletonTargetFormatter : ObjectFormatter<SingletonTarget>
    {
		public override string Format(SingletonTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			if (obj.DeclaredType != obj.InnerTarget.DeclaredType)
				return string.Format(formatters, "{{ {0} Singleton: {1} }}", obj.DeclaredType, obj.InnerTarget);
			else
				return string.Format(formatters, "{{ Singleton: {0} }}", obj.InnerTarget);
		}
	}
}
