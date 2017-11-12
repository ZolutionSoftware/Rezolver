// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


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
            return properties.Where(PropertyInfoExtensions.IsWritable);
        }

        public static IEnumerable<PropertyInfo> PubliclyWritable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(PropertyInfoExtensions.IsPubliclyWritable);
        }

        /// <summary>
        /// Filters the property enumerable down to those that have get accessors of any visibility.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> Readable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(PropertyInfoExtensions.IsReadable);
        }

        public static IEnumerable<PropertyInfo> PubliclyReadable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(PropertyInfoExtensions.IsPubliclyReadable);
        }
    }
}
