using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reflection
{
	internal static class FieldInfoEnumerableExtensions
	{
		public static IEnumerable<FieldInfo> Public(this IEnumerable<FieldInfo> fields)
		{
			fields.MustNotBeNull(nameof(fields));
			return fields.Where(f => f.IsPublic);
		}
	}
}
