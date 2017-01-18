// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rezolver
{
    internal static class TypeInfoExtensions
    {
		internal static IEnumerable<Type> GetAllBases(this TypeInfo type)
		{
			type.MustNotBeNull(nameof(type));
			var baseType = type.BaseType;
			while (baseType != null)
			{
				yield return baseType;
				baseType = TypeHelpers.BaseType(baseType);
			}
		}
    }
}
