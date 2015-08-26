using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reflection
{
	public static class PropertyInfoEnumerableExtensions
	{
		public static IEnumerable<PropertyInfo> PubliclyWritable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Where(p => p.CanWrite && p.SetMethod.IsPublic);
#else
			return properties.Where(p => p.CanWrite && p.GetSetMethod().IsPublic);
#endif
		}

		public static IEnumerable<PropertyInfo> PubliclyReadable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Where(p => p.CanRead && p.GetMethod.IsPublic);
#else
			return properties.Where(p => p.CanRead && p.GetGetMethod().IsPublic);
#endif
		}
	}
}
