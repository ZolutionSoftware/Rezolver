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
