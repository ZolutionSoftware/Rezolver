using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rezolver
{
    internal static class FetchDirectITargetContainerExtensions
    {
        internal static TObj FetchDirect<TObj>(this ITargetContainer targets)
        {
            return (TObj)targets.FetchDirect(typeof(TObj));
        }

        internal static object FetchDirect(this ITargetContainer targets, Type objectType)
        {
            var result = targets.Fetch(objectType);
            if (result != null)
                return FetchDirect(result, targets, objectType);
            return result;
        }

        private static object FetchDirect(ITarget target, ITargetContainer targets, Type objectType)
        {
            if (TypeHelpers.IsAssignableFrom(objectType, target.GetType()))
                return target;

            if (target is IDirectTarget directTarget)
                return directTarget.GetValue();

            return null;
        }

        internal static IEnumerable<TObj> FetchAllDirect<TObj>(this ITargetContainer targets)
        {
            return targets.FetchAll(typeof(TObj))
                .Select(t => FetchDirect(t, targets, typeof(TObj)))
                .Where(r => r != null)
                .OfType<TObj>();
        }

        internal static IEnumerable<object> FetchAllDirect(this ITargetContainer targets, Type objectType)
        {
            return targets.FetchAll(objectType)
                .Select(t => FetchDirect(t, targets, objectType))
                .Where(r => r != null);
        }
    }
}
