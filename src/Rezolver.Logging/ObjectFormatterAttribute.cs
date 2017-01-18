// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rezolver.Logging
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ObjectFormatterAttribute : Attribute
    {
		public Type[] AssociatedTypes { get; }

		public ObjectFormatterAttribute(params Type[] associatedTypes)
		{
			AssociatedTypes = associatedTypes;
		}
    }
}
