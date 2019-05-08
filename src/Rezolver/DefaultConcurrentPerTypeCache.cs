// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;

namespace Rezolver
{

    internal sealed class PerTypeCache<TValue>
    {
        public const int DEFAULT_CAPACITY = 512;
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        private readonly ConcurrentDictionary<Type, TValue> _entries;
        private readonly Func<Type, TValue> _creationCallback;

        public PerTypeCache(Func<Type, TValue> creationCallback)
            : this(creationCallback, DEFAULT_CAPACITY)
        {

        }

        public PerTypeCache(Func<Type, TValue> creationCallback, int capacity)
            : this(creationCallback, DefaultConcurrencyLevel, capacity)
        {

        }

        public PerTypeCache(Func<Type, TValue> creationCallback, int concurrencyLevel, int capacity)
        {
            _entries = new ConcurrentDictionary<Type, TValue>(concurrencyLevel, capacity);
            this._creationCallback = creationCallback;
        }

        public TValue Get(Type type)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            if (this._entries.TryGetValue(type, out var value))
                return value;

            return this._entries.GetOrAdd(type, _creationCallback);
        }
    }

    internal sealed class PerResolveContextCache<TValue>
    {
        public const int DEFAULT_CAPACITY = 512;
        public static int DefaultConcurrencyLevel => Environment.ProcessorCount;

        private readonly ConcurrentDictionary<ResolveContext, TValue> _entries;
        private readonly Func<ResolveContext, TValue> _creationCallback;

        public PerResolveContextCache(Func<ResolveContext, TValue> creationCallback)
            : this(creationCallback, DEFAULT_CAPACITY)
        {

        }

        public PerResolveContextCache(Func<ResolveContext, TValue> creationCallback, int capacity)
            : this(creationCallback, DefaultConcurrencyLevel, capacity)
        {

        }

        public PerResolveContextCache(Func<ResolveContext, TValue> creationCallback, int concurrencyLevel, int capacity)
        {
            _entries = new ConcurrentDictionary<ResolveContext, TValue>(concurrencyLevel, capacity);
            this._creationCallback = creationCallback;
        }

        public TValue Get(ResolveContext context)
        {
            // Implementation note: after extensive testing, we found that this approach is faster in the best
            // case than always using GetOrAdd - that is: once a compiled target is built and cached, TryGetValue
            // performs better than GetOrAdd.
            if (this._entries.TryGetValue(context, out var value))
                return value;

            return this._entries.GetOrAdd(context, _creationCallback);
        }
    }
}
