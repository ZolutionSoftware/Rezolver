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
    internal sealed class DefaultConcurrentPerTypeCache<TValue> : IConcurrentPerTypeCache<TValue>
    {
        public const int DEFAULT_CAPACITY = 31;
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        private readonly ConcurrentDictionary<Type, Lazy<TValue>> _entries;

        // cache the very last result in cases where the application is hammering away at the same type
        private (Type type, TValue value) _last;

        public DefaultConcurrentPerTypeCache()
            : this(DEFAULT_CAPACITY)
        {
            
        }

        public DefaultConcurrentPerTypeCache(int capacity)
            : this(DefaultConcurrencyLevel, capacity)
        {

        }

        public DefaultConcurrentPerTypeCache(int concurrencyLevel, int capacity)
        {
            _entries = new ConcurrentDictionary<Type, Lazy<TValue>>(concurrencyLevel, capacity);
        }

        #region fastest (needs more validation)
        public TValue Get_fastest(Type type, Func<TValue> valueFactory)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            Type t;
            TValue cached;
            (t, cached) = _last;

            if (type == t)
                return cached;

            t = type;

            if (!this._entries.TryGetValue(type, out Lazy<TValue> myLazy))
            {
                //return this._entries.GetOrAdd(
                //    type,
                //    c => new Lazy<TValue>(() => valueFactory())).Value;
                cached = this._entries.GetOrAdd(
                    type, new Lazy<TValue>(valueFactory)).Value;
            }
            else
            {
                //return myLazy.Value;
                cached = myLazy.Value;
            }

            _last = (type, cached);
            return cached;
        }

        #endregion

        #region fast (safest)
        public TValue Get(Type type, Func<TValue> valueFactory)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            if (this._entries.TryGetValue(type, out Lazy<TValue> myLazy))
                return myLazy.Value;

            return this._entries.GetOrAdd(type, new Lazy<TValue>(valueFactory)).Value;
        }

        #endregion
    }

    /// <summary>
    /// The worst performing
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class ReadWriteLockedConcurrentPerTypeCache<TValue> : IConcurrentPerTypeCache<TValue>
    {
        
        public const int DEFAULT_CAPACITY = 31;

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<Type, TValue> _entries;

        public ReadWriteLockedConcurrentPerTypeCache()
            : this(DEFAULT_CAPACITY)
        {

        }

        public ReadWriteLockedConcurrentPerTypeCache(int capacity)
        {
            _entries = new Dictionary<Type, TValue>(capacity);
        }

        public TValue Get(Type type, Func<TValue> valueFactory)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_entries.TryGetValue(type, out TValue value))
                    return value;

                _lock.EnterWriteLock();
                try
                {
                    return _entries[type] = valueFactory();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }
    }

    /// <summary>
    /// Not bad performance, but not as good as concurrentdictionary
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    internal sealed class LockedConcurrentPerTypeCache<TValue> : IConcurrentPerTypeCache<TValue>
    {
        public const int DEFAULT_CAPACITY = 31;

        private readonly object _lock = new object();
        private readonly Dictionary<Type, TValue> _entries;

        public LockedConcurrentPerTypeCache()
            : this(DEFAULT_CAPACITY)
        {

        }

        public LockedConcurrentPerTypeCache(int capacity)
        {
            _entries = new Dictionary<Type, TValue>(capacity);
        }

        public TValue Get(Type type, Func<TValue> valueFactory)
        {
            lock(_lock)
            {
                if (_entries.TryGetValue(type, out TValue value))
                    return value;

                return _entries[type] = valueFactory();
                
            }
        }
    }
}
