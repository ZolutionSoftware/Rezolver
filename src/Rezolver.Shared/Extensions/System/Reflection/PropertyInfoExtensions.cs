using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Reflection
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsWritable(this PropertyInfo pi)
        {
#if DOTNET
            return pi.CanWrite && pi.SetMethod != null;
#else
            return pi.CanWrite && pi.GetSetMethod() != null;
#endif
        }

        public static bool IsPubliclyWritable(this PropertyInfo pi)
        {
#if DOTNET
            return pi.CanWrite && (pi.SetMethod?.IsPublic ?? false);
#else
            return pi.CanWrite && (pi.GetSetMethod()?.IsPublic ?? false);
#endif
        }

        public static bool IsReadable(this PropertyInfo pi)
        {
#if DOTNET
            return pi.CanRead && pi.GetMethod != null;
#else
            return pi.CanRead && pi.GetGetMethod() != null;
#endif
        }

        public static bool IsPubliclyReadable(this PropertyInfo pi)
        {
#if DOTNET
            return pi.CanRead && (pi.GetMethod?.IsPublic ?? false);
#else
            return pi.CanRead && (pi.GetGetMethod()?.IsPublic ?? false);
#endif
        }

    }
}
