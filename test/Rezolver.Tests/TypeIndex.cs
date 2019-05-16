using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Rezolver.Tests
{
    [DebuggerDisplay("{CachedMetadata}")]
    public class TypeIndex
    {
        // might need to be a Lazy<TargetTypeMetadata>...
        private protected PerTypeCache<TypeIndexEntry> CachedMetadata { get; }

        private TypeIndexEntry CreateMetadata(Type t)
        {
            if (!t.ContainsGenericParameters)
                return (TypeIndexEntry)Activator.CreateInstance(typeof(TypeIndexEntry<>).MakeGenericType(t), this);
            else
                return (TypeIndexEntry)Activator.CreateInstance(typeof(TypeIndexGenericEntry), this, t);
        }

        public TypeIndex()
        {
            CachedMetadata = new PerTypeCache<TypeIndexEntry>(CreateMetadata);
        }

        public TypeIndexEntry For<T>()
        {
            return CachedMetadata.Get(typeof(T));
        }

        public TypeIndexEntry For(Type type)
        {
            return CachedMetadata.Get(type);
        }

        /// <summary>
        /// Ensures metadata is built for the type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The type whose metadata is required.</typeparam>
        public void Prepare<T>()
        {
            CachedMetadata.Get(typeof(T));
        }

        public void Prepare(params Type[] types) => Prepare((IEnumerable<Type>)types);

        public void Prepare(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                CachedMetadata.Get(type);
            }
        }
    }
}
