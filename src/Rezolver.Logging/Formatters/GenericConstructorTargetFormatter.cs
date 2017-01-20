// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
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
