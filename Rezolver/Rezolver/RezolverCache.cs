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

		public Func<object> GetStaticFactory(Type type, Func<Type, string, IRezolveTarget> targetFactory, string name = null)
		{
			CacheEntry cacheEntry = name == null
				? GetCacheEntry(type, targetFactory)
				: GetCacheEntry(new CacheKey(type, name), targetFactory);

			return cacheEntry != null ? cacheEntry.StaticFactory : null;
		}

		public Func<IRezolverContainer, object> GetDynamicFactory(Type type, Func<Type, string, IRezolveTarget> targetFactory, string name = null)
		{
			CacheEntry cacheEntry = name == null
				? GetCacheEntry(type, targetFactory)
				: GetCacheEntry(new CacheKey(type, name), targetFactory);

			return cacheEntry != null ? cacheEntry.DynamicFactory : null;
		}

		public Func<T> GetStaticFactory<T>(Func<Type, string, IRezolveTarget> targetFactory, string name = null)
		{
			var cacheEntry = (name == null
				? GetCacheEntry(typeof(T), targetFactory)
				: GetCacheEntry(new CacheKey(typeof(T), name), targetFactory)) as CacheEntry<T>;

			return cacheEntry != null ? cacheEntry.StrongStaticFactory : null;
		}

		public Func<IRezolverContainer, T> GetDynamicFactory<T>(Func<Type, string, IRezolveTarget> targetFactory, string name = null)
		{
			var cacheEntry = (name == null
				? GetCacheEntry(typeof(T), targetFactory)
				: GetCacheEntry(new CacheKey(typeof(T), name), targetFactory)) as CacheEntry<T>;

			return cacheEntry != null ? cacheEntry.StrongDynamicFactory : null;
		}

		private CacheEntry GetCacheEntry(Type type, Func<Type, string, IRezolveTarget> targetFactory)
		{
			CacheEntry toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
			var target = targetFactory(type, null);

			if (target != null)
				return _typeOnlyCacheEntries[type] = CacheEntry.MakeCacheEntry(type, target, _container, _compiler,
					ExpressionHelper.DynamicContainerParam);
			else
				return _typeOnlyCacheEntries[type] = CacheEntry.Miss;
		}

		private CacheEntry GetCacheEntry(CacheKey key, Func<Type, string, IRezolveTarget> targetFactory)
		{
			CacheEntry toReturn;
			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			var target = targetFactory(key.Type, key.Name);

			if (target != null)
				return
					_namedCacheEntries[key] =
						CacheEntry.MakeCacheEntry(key.Type, target, _container, _compiler, ExpressionHelper.DynamicContainerParam);
			else
				return _namedCacheEntries[key] = CacheEntry.Miss;
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
			public static readonly CacheEntry Miss = new CacheEntry();

			private static readonly
				Dictionary<Type, Lazy<MakeCacheEntryDelegate>>
				LateBoundCacheEntryLookup =
					new Dictionary
						<Type, Lazy<MakeCacheEntryDelegate>>(); 

			//note here - the signature of the static container is the same as the dynamic one, 
			//but you should be aware that a static container will simply ignore any dynamic
			//container that's passed to it when executed.

			private readonly Func<object> _staticFactory;
			private readonly Func<IRezolverContainer, object> _dynamicFactory;
			private readonly bool _isMiss;

			private static readonly MethodInfo MakeCacheEntryStaticGeneric = typeof (CacheEntry).GetMethods(BindingFlags.Public | BindingFlags.Static)
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

			protected CacheEntry()
			{
				_isMiss = true;
			}

			public CacheEntry(Func<object> staticFactory, Func<IRezolverContainer, object> dynamicFactory)
			{
				_staticFactory = staticFactory;
				_dynamicFactory = dynamicFactory;
			}

			public static CacheEntry MakeCacheEntry(Type returnType, IRezolveTarget target, IRezolverContainer container,
				IRezolverTargetCompiler compiler, ParameterExpression dynamicContainerExpression)
			{
				Lazy<MakeCacheEntryDelegate> lazy = null;
				//TODO: add a lock.
				if (!LateBoundCacheEntryLookup.TryGetValue(returnType, out lazy))
				{
					LateBoundCacheEntryLookup[returnType] = lazy = new Lazy<MakeCacheEntryDelegate>(
						() => (MakeCacheEntryDelegate)Delegate.CreateDelegate(typeof (MakeCacheEntryDelegate),
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
		}

		public class CacheEntry<TTarget> : CacheEntry 
		{
			private readonly Func<TTarget> _strongStaticFactory;
			private readonly Func<IRezolverContainer, TTarget> _strongDynamicFactory;

			public Func<TTarget> StrongStaticFactory
			{
				get { return _strongStaticFactory; }
			}

			public Func<IRezolverContainer, TTarget> StrongDynamicFactory
			{
				get { return _strongDynamicFactory; }
			}

			protected CacheEntry()
			{
			}

			public CacheEntry(Func<TTarget> strongStaticFactory, Func<IRezolverContainer, TTarget> strongDynamicFactory,
				Func<object> staticFactory, Func<IRezolverContainer, object> dynamicFactory)
				: base(staticFactory, dynamicFactory)
			{
				_strongStaticFactory = strongStaticFactory;
				_strongDynamicFactory = strongDynamicFactory;
			}
		}
	}
}