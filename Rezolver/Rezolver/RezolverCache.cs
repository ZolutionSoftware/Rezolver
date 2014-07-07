using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	using MakeCacheEntryDelegate = Func<IRezolveTarget, IRezolverContainer, IRezolverTargetCompiler, ParameterExpression, RezolverCache.CacheEntry>;

	public class RezolverCache
	{
		private readonly IRezolverContainer _container;
		private readonly IRezolverTargetCompiler _compiler;

		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, CacheEntry> _typeOnlyCacheEntries = new Dictionary<Type, CacheEntry>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<CacheKey, CacheEntry> _namedCacheEntries = new Dictionary<CacheKey, CacheEntry>();

		public RezolverCache(IRezolverContainer container, IRezolverTargetCompiler compiler)
		{
			_container = container;
			_compiler = compiler;
		}

		public bool HasFactory(Type type, string name = null)
		{
			CacheEntry toCheck;
			return name != null
				? _namedCacheEntries.ContainsKey(new CacheKey(type, name))
				: _typeOnlyCacheEntries.ContainsKey(type);
		}

		public Func<object> GetStaticFactory(Type type, string name = null)
		{
			CacheEntry cacheEntry = name == null
				? GetCacheEntry(type)
				: GetCacheEntry(new CacheKey(type, name));

			return cacheEntry != null ? cacheEntry.StaticFactory : null;
		}

		public Func<IRezolverContainer, object> GetDynamicFactory(Type type, string name = null)
		{
			CacheEntry cacheEntry = name == null
				? GetCacheEntry(type)
				: GetCacheEntry(new CacheKey(type, name));

			return cacheEntry != null ? cacheEntry.DynamicFactory : null;
		}

		public Func<T> GetStaticFactory<T>(string name = null)
		{
			var cacheEntry = (name == null
				? GetCacheEntry(typeof(T))
				: GetCacheEntry(new CacheKey(typeof(T), name))) as CacheEntry<T>;

			return cacheEntry != null ? cacheEntry.StrongStaticFactory : null;
		}

		public Func<IRezolverContainer, T> GetDynamicFactory<T>(string name = null)
		{
			var cacheEntry = (name == null
				? GetCacheEntry(typeof(T))
				: GetCacheEntry(new CacheKey(typeof(T), name))) as CacheEntry<T>;

			return cacheEntry != null ? cacheEntry.StrongDynamicFactory : null;
		}

		private CacheEntry GetCacheEntry(Type type)
		{
			CacheEntry toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
			var target = _container.Fetch(type, null);

			if (target != null)
				return _typeOnlyCacheEntries[type] = CacheEntry.MakeCacheEntry(type, target, _container, _compiler,
					ExpressionHelper.DynamicContainerParam);
			else
				return _typeOnlyCacheEntries[type] = CacheEntry.MakeCacheMiss(type);
		}

		private CacheEntry GetCacheEntry(CacheKey key)
		{
			CacheEntry toReturn;
			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			var target = _container.Fetch(key.Type, key.Name);

			if (target != null)
				return
					_namedCacheEntries[key] =
						CacheEntry.MakeCacheEntry(key.Type, target, _container, _compiler, ExpressionHelper.DynamicContainerParam);
			else
				return _namedCacheEntries[key] = CacheEntry.MakeCacheMiss(key.Type);
		}

		public struct CacheKey : IEquatable<CacheKey>
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
				if (obj == null || !(obj is CacheKey))
					return false;
				return Equals((CacheKey)obj);
			}

			public bool Equals(CacheKey other)
			{
				return _type == other._type && _name == other._name;
			}

			public static bool operator ==(CacheKey left, CacheKey right)
			{
				return left._type == right._type && left._name == right._name;
			}

			public static bool operator !=(CacheKey left, CacheKey right)
			{
				return left._type != right._type || left._name != right._name;
			}
		}

		public class CacheEntry
		{
			private static readonly
				Dictionary<Type, Lazy<MakeCacheEntryDelegate>>
				LateBoundCacheEntryLookup =
					new Dictionary
						<Type, Lazy<MakeCacheEntryDelegate>>();

			private static readonly
				Dictionary<Type, CacheEntry>
				CacheMisses = new Dictionary<Type, CacheEntry>();

			//note here - the signature of the static container is the same as the dynamic one, 
			//but you should be aware that a static container will simply ignore any dynamic
			//container that's passed to it when executed.

			private readonly Func<object> _staticFactory;
			private readonly Func<IRezolverContainer, object> _dynamicFactory;
			private readonly bool _isMiss;

			private static readonly MethodInfo MakeCacheEntryStaticGeneric = typeof(CacheEntry).GetMethods(BindingFlags.Public | BindingFlags.Static)
				.SingleOrDefault(mi => mi.Name == "MakeCacheEntry" && mi.IsGenericMethod);

			public bool IsMiss
			{
				get { return _isMiss; }
			}

			public Func<object> StaticFactory
			{
				get { return _staticFactory; }
			}

			public Func<IRezolverContainer, object> DynamicFactory
			{
				get { return _dynamicFactory; }
			}

			protected CacheEntry(Type target)
			{
				_isMiss = true;
				_staticFactory = () => ResolveFailure(target);
				_dynamicFactory = (container) => ResolveFailureDynamic(target, container);
			}

			public CacheEntry(Func<object> staticFactory, Func<IRezolverContainer, object> dynamicFactory)
			{
				_staticFactory = staticFactory;
				_dynamicFactory = dynamicFactory;
			}

			private static object ResolveFailure(Type type)
			{
				//TODO: Localise this string
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", type));
			}

			private static object ResolveFailureDynamic(Type type, IRezolverContainer container)
			{
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", type));
			}

			public static CacheEntry MakeCacheEntry(Type returnType, IRezolveTarget target, IRezolverContainer container,
				IRezolverTargetCompiler compiler, ParameterExpression dynamicContainerExpression)
			{
				Lazy<MakeCacheEntryDelegate> lazy = null;
				//TODO: add a lock.
				if (!LateBoundCacheEntryLookup.TryGetValue(returnType, out lazy))
				{
					LateBoundCacheEntryLookup[returnType] = lazy = new Lazy<MakeCacheEntryDelegate>(
						() => (MakeCacheEntryDelegate)Delegate.CreateDelegate(typeof(MakeCacheEntryDelegate),
							MakeCacheEntryStaticGeneric.MakeGenericMethod(returnType)));
				}

				return lazy.Value(target, container, compiler, dynamicContainerExpression);
			}

			public static CacheEntry MakeCacheEntry<TTarget>(IRezolveTarget target, IRezolverContainer container,
				IRezolverTargetCompiler compiler, ParameterExpression dynamicContainerExpression)
			{
				//yup - compiling every version that we might need in advance.
				return new CacheEntry<TTarget>(compiler.CompileStatic<TTarget>(target, container), compiler.CompileDynamic<TTarget>(target, container, dynamicContainerExpression: dynamicContainerExpression),
					compiler.CompileStatic(target, container, typeof(TTarget)), compiler.CompileDynamic(target, container, dynamicContainerExpression: dynamicContainerExpression, targetType: typeof(TTarget)));
			}

			public static CacheEntry MakeCacheMiss(Type target)
			{
				CacheEntry result = null;

				if (CacheMisses.TryGetValue(target, out result))
					return result;

				Type cacheMissType = typeof(CacheEntry<>).MakeGenericType(target);
				return CacheMisses[target] = (CacheEntry)Activator.CreateInstance(target);
			}
		}

		public class CacheEntry<TTarget> : CacheEntry
		{
			private readonly Func<TTarget> _strongStaticFactory;
			private readonly Func<IRezolverContainer, TTarget> _strongDynamicFactory;

			public static readonly CacheEntry<TTarget> Miss = new CacheEntry<TTarget>();

			public Func<TTarget> StrongStaticFactory
			{
				get { return _strongStaticFactory; }
			}

			public Func<IRezolverContainer, TTarget> StrongDynamicFactory
			{
				get { return _strongDynamicFactory; }
			}

			/// <summary>
			/// Used only to create a cache miss - all of the faactories exposed by this entry will 
			/// throw an exception if executed.
			/// </summary>
			protected CacheEntry() : base(typeof(TTarget))
			{
				//TODO: Make cache entry delegates in CacheMiss type throw exceptions so upstream callers don't have
				//to check for null/IsMiss - they can simply call the delegates.
				_strongStaticFactory = ResolveFailure;
				_strongDynamicFactory = ResolveFailureDynamic;
			}

			private static TTarget ResolveFailure()
			{
				//TODO: Localise this string
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", typeof(TTarget)));
			}

			private static TTarget ResolveFailureDynamic(IRezolverContainer container)
			{
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", typeof(TTarget)));
			}

			public CacheEntry(Func<TTarget> strongStaticFactory, Func<IRezolverContainer, TTarget> strongDynamicFactory,
				Func<object> staticFactory, Func<IRezolverContainer, object> dynamicFactory)
				: base(staticFactory, dynamicFactory)
			{
				_strongStaticFactory = strongStaticFactory;
				_strongDynamicFactory = strongDynamicFactory;
			}

			public static CacheEntry<TTarget> CreateMiss()
			{
				return new CacheEntry<TTarget>();
			}
		}
	}
}