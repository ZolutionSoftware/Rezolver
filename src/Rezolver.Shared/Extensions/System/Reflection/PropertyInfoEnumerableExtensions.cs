using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Reflection
{
	internal static class PropertyInfoEnumerableExtensions
	{
		public static IEnumerable<PropertyInfo> Writable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Where(p => p.CanWrite && p.SetMethod != null);
#else
			return properties.Where(p => p.CanWrite && p.GetSetMethod() != null);
#endif
		}

		public static IEnumerable<PropertyInfo> PubliclyWritable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Writable().Where(p => p.SetMethod.IsPublic);
#else
			return properties.Writable().Where(p => p.GetSetMethod().IsPublic);
#endif
		}

		/// <summary>
		/// Filters the property enumerable down to those that have get accessors of any visibility.
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyInfo> Readable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Where(p => p.CanRead && p.GetMethod != null);
#else
			return properties.Where(p => p.CanRead && p.GetGetMethod() != null);
#endif
		}

		public static IEnumerable<PropertyInfo> PubliclyReadable(this IEnumerable<PropertyInfo> properties)
		{
#if DOTNET
			return properties.Readable().Where(p => p.GetMethod.IsPublic);
#else
			return properties.Readable().Where(p => p.GetGetMethod().IsPublic);
#endif
		}
	}
}
