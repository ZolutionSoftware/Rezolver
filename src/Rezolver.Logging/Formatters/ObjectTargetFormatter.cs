﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
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
