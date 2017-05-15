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
        private class NullContainer : IContainer
        {
            public static NullContainer Instance { get; } = new NullContainer();
            private NullContainer() { }
            public bool CanResolve(IResolveContext context)
            {
                throw new NotImplementedException();
            }

            public IContainerScope CreateScope()
            {
                throw new NotImplementedException();
            }

            public ICompiledTarget FetchCompiled(IResolveContext context)
            {
                throw new NotImplementedException();
            }

            public object GetService(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public object Resolve(IResolveContext context)
            {
                throw new NotImplementedException();
            }

            public bool TryResolve(IResolveContext context, out object result)
            {
                throw new NotImplementedException();
            }
        }
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

            if (target is ICompiledTarget resultCompiledTarget)
                return resultCompiledTarget.GetObject(new ResolveContext(NullContainer.Instance, objectType));

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
