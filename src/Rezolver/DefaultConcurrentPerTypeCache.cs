using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rezolver
{
    /// <summary>
    /// Best, general-purpose cache
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class PerTypeCache<TValue>
    {
        public const int DEFAULT_CAPACITY = 512;
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        private readonly ConcurrentDictionary<Type, Lazy<TValue>> _entries;

        public PerTypeCache()
            : this(DEFAULT_CAPACITY)
        {
            
        }

        public PerTypeCache(int capacity)
            : this(DefaultConcurrencyLevel, capacity)
        {

        }

        public PerTypeCache(int concurrencyLevel, int capacity)
        {
            _entries = new ConcurrentDictionary<Type, Lazy<TValue>>(concurrencyLevel, capacity);
        }

        public TValue Get(Type type, Func<TValue> valueFactory)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            if (this._entries.TryGetValue(type, out Lazy<TValue> myLazy))
                return myLazy.Value;

            return this._entries.GetOrAdd(type, new Lazy<TValue>(valueFactory)).Value;
        }
    }

    internal sealed class PerResolveContextCache<TValue>
    {
        private class ResolveContextTypeComparer : IEqualityComparer<ResolveContext>
        {
            public static readonly ResolveContextTypeComparer Instance = new ResolveContextTypeComparer();

            public bool Equals(ResolveContext x, ResolveContext y)
            {
                return x.RequestedType == y.RequestedType;
            }

            public int GetHashCode(ResolveContext obj)
            {
                return obj.RequestedType.GetHashCode();
            }
        }

        public const int DEFAULT_CAPACITY = 512;
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        private readonly ConcurrentDictionary<ResolveContext, TValue> _entries;

        public PerResolveContextCache()
            : this(DEFAULT_CAPACITY)
        {

        }

        public PerResolveContextCache(int capacity)
            : this(DefaultConcurrencyLevel, capacity)
        {

        }

        public PerResolveContextCache(int concurrencyLevel, int capacity)
        {
            _entries = new ConcurrentDictionary<ResolveContext, TValue>(concurrencyLevel, capacity, ResolveContextTypeComparer.Instance);
        }

        public TValue Get(ResolveContext context, Func<ResolveContext, TValue> valueFactory)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            if (this._entries.TryGetValue(context, out var value))
                return value;

            return this._entries.GetOrAdd(context, valueFactory);
        }
    }
}
