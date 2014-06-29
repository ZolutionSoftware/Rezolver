using System;
using System.Collections.Generic;

namespace Rezolver
{
	public class RezolverCache
	{
		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, CacheEntry> _typeOnlyCacheEntries = new Dictionary<Type, CacheEntry>(); 
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<CacheKey, CacheEntry> _namedCacheEntries = new Dictionary<CacheKey, CacheEntry>();

		public RezolverCache()
		{
		
		}

		public bool HasFactory(Type type, string name = null)
		{
			CacheEntry toCheck;
			return name != null
				? _namedCacheEntries.ContainsKey(new CacheKey(type, name))
				: _typeOnlyCacheEntries.ContainsKey(type);
		}

		public Func<IRezolverContainer, object> GetFactory(Type type, Func<Type, string, Func<IRezolverContainer, object>> createFactory, string name = null)
		{
			CacheEntry cacheEntry = name == null
				? GetCacheEntry(type, createFactory)
				: GetCacheEntry(new CacheKey(type, name), createFactory);

			return cacheEntry != null ? cacheEntry.Factory : null;
		}

		private CacheEntry GetCacheEntry(Type type, Func<Type, string, Func<IRezolverContainer, object>> createFactory)
		{
			CacheEntry toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
			return _typeOnlyCacheEntries[type] = new CacheEntry(createFactory(type, null));
		}

		private CacheEntry GetCacheEntry(CacheKey key, Func<Type, string, Func<IRezolverContainer, object>> createFactory)
		{
			CacheEntry toReturn;

			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			return _namedCacheEntries[key] = new CacheEntry(createFactory(key.Type, key.Name));
		}

		public class CacheKey : IEquatable<CacheKey>
		{
			private readonly Type _type;
			private readonly string _name;

			public Type Type { get { return _type; } }
			public string Name { get { return _name; } }

			public CacheKey(Type type, string name)
			{
				_type = type;
				_name = name;
			}

			public override int GetHashCode()
			{
				return _type.GetHashCode() ^ (_name != null ? _name.GetHashCode() : 0);
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as CacheKey);
			}

			public bool Equals(CacheKey other)
			{
				if (other != null)
				{
					return _type == other._type && _name == other._name;
				}
				return false;
			}
		}

		public class CacheEntry
		{
			private readonly Func<IRezolverContainer, object> _factory;
			public static readonly CacheEntry Miss = new CacheEntry();

			public bool IsMiss
			{
				get { return Miss == this; }
			}

			public Func<IRezolverContainer, object> Factory
			{
				get { return _factory; }
			}


			private CacheEntry()
			{
			}


			public CacheEntry(Func<IRezolverContainer, object> factory)
			{
				_factory = factory;
			}
		}
	}
}