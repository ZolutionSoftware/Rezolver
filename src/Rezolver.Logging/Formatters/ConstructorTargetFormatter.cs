// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging.Formatters
{
	[ObjectFormatter]
	public class ConstructorTargetFormatter : ObjectFormatter<ConstructorTarget>
	{
		public override string Format(ConstructorTarget obj, string format = null, ObjectFormatterCollection formatters = null)
		{
			string argsString = null;
			if (obj.ParameterBindings.Count != 0)
				argsString = string.Join(", ", obj.ParameterBindings.Select(pb => string.Format(formatters, "{0}={1}", pb.Parameter, pb.Target)));
			else if (obj.NamedArgs.Count != 0)
				argsString = string.Join(", ", obj.NamedArgs.Select(a => string.Format(formatters, "{0}:{1}", a.Key, a.Value)));
			else if (obj.Ctor != null)
				argsString = formatters.Format(obj.Ctor);
			else
				argsString = "Auto"; //late-bound Constructor with all-resolved or default arguments
			return string.Format(formatters, "{{ New {0}({1}) }}", 
				obj.DeclaredType,
				argsString);
		}
	}
}
