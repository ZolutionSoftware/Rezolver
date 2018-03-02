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
        internal static IEnumerable<PropertyInfo> Writable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(RzPropertyInfoExtensions.IsWritable);
        }

        internal static IEnumerable<PropertyInfo> PubliclyWritable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(RzPropertyInfoExtensions.IsPubliclyWritable);
        }

        internal static IEnumerable<PropertyInfo> Readable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(RzPropertyInfoExtensions.IsReadable);
        }

        internal static IEnumerable<PropertyInfo> PubliclyReadable(this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(RzPropertyInfoExtensions.IsPubliclyReadable);
        }
    }
}
