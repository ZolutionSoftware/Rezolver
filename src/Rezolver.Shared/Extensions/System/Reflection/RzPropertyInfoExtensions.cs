// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    internal static class RzPropertyInfoExtensions
    {
        public static bool IsWritable(this PropertyInfo pi)
        {
            return pi.CanWrite && pi.GetSetMethod() != null;
        }

        public static bool IsPubliclyWritable(this PropertyInfo pi)
        {
            return pi.CanWrite && (pi.GetSetMethod()?.IsPublic ?? false);
        }

        public static bool IsReadable(this PropertyInfo pi)
        {
            return pi.CanRead && pi.GetGetMethod() != null;
        }

        public static bool IsPubliclyReadable(this PropertyInfo pi)
        {
            return pi.CanRead && (pi.GetGetMethod()?.IsPublic ?? false);
        }
    }
}
