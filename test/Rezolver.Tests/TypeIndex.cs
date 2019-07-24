using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rezolver.Tests
{
    [DebuggerDisplay("{CachedMetadata}")]
    public class TypeIndex
    {
        private protected PerTypeCache<TypeIndexEntry> CachedMetadata { get; }

        public TypeIndex()
        {
            CachedMetadata = new PerTypeCache<TypeIndexEntry>(t => new TypeIndexEntry(this, t));
        }

        public TypeIndexEntry For<T>()
        {
            return For(typeof(T), new Dictionary<Type, TypeIndexEntry>());
        }

        public TypeIndexEntry For(Type type)
        {
            return For(type, new Dictionary<Type, TypeIndexEntry>());
        }

        internal TypeIndexEntry For(Type type, Dictionary<Type, TypeIndexEntry> beingBuilt)
        {
            if (beingBuilt.TryGetValue(type, out var alreadyBuilt))
                return alreadyBuilt;

            var result = CachedMetadata.Get(type);
            beingBuilt[type] = result;
            result.Initialise(beingBuilt);

            return result;
        }

        /// <summary>
        /// Ensures metadata is built for the type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type whose metadata is required.</typeparam>
        public void Prepare<T>()
        {
            For<T>();//CachedMetadata.Get(typeof(T));
        }

        public void Prepare(params Type[] types)
        {
            Prepare((IEnumerable<Type>)types);
        }

        public void Prepare(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                For(type);
            }
        }

        public bool Has<T>()
        {
            return CachedMetadata.Contains(typeof(T));
        }

        public bool Has(Type type)
        {
            return CachedMetadata.Contains(type);
        }
    }
}
