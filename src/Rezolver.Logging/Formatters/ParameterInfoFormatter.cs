// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
	public class ParameterInfoFormatter : ObjectFormatter<ParameterInfo>
	{
		public override string Format(ParameterInfo obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			return string.Format(formatters, "{0} {1}", obj.ParameterType, obj.Name);
		}
	}
}
