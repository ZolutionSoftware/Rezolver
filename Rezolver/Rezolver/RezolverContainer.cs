using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rezolver
{
	using MakeCacheEntryDelegate = Func<IRezolveTarget, RezolverContainer, RezolverContainer.ICacheEntry>;
	/// <summary>
	/// </summary>
	public class RezolverContainer : IRezolverContainer
	{
		#region nested cache types

		private static readonly MethodInfo MakeCacheEntryStaticGeneric = typeof(RezolverContainer).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
			.SingleOrDefault(mi => mi.Name == "MakeCacheEntry" && mi.IsGenericMethod);

		//note here - the signature of the static container is the same as the dynamic one, 
		//but you should be aware that a static container will simply ignore any dynamic
		//container that's passed to it when executed.

		private static readonly
				Dictionary<Type, Lazy<MakeCacheEntryDelegate>>
				LateBoundCacheEntryLookup =
					new Dictionary
						<Type, Lazy<MakeCacheEntryDelegate>>();

		private static readonly
			Dictionary<Type, ICacheEntry>
			CacheMisses = new Dictionary<Type, ICacheEntry>();

		private static ICacheEntry MakeCacheEntry(Type returnType, IRezolveTarget target, RezolverContainer container)
		{
			Lazy<MakeCacheEntryDelegate> lazy = null;
			//TODO: add a lock.
			if (!LateBoundCacheEntryLookup.TryGetValue(returnType, out lazy))
			{
				LateBoundCacheEntryLookup[returnType] = lazy = new Lazy<MakeCacheEntryDelegate>(
					() => (MakeCacheEntryDelegate)Delegate.CreateDelegate(typeof(MakeCacheEntryDelegate),
						MakeCacheEntryStaticGeneric.MakeGenericMethod(returnType)));
			}

			return lazy.Value(target, container);
		}

		private static ICacheEntry MakeCacheEntry<TTarget>(IRezolveTarget target, RezolverContainer container)
		{
			//yup - compiling every version that we might need in advance.
			return new CacheEntry<TTarget>(container.Compiler.CompileStatic<TTarget>(target, container), container.Compiler.CompileDynamic<TTarget>(target, container, dynamicContainerExpression: ExpressionHelper.DynamicContainerParam),
				container.Compiler.CompileStatic(target, container, typeof(TTarget)), container.Compiler.CompileDynamic(target, container, dynamicContainerExpression: ExpressionHelper.DynamicContainerParam, targetType: typeof(TTarget)));
		}

		private static ICacheEntry MakeCacheMiss(Type target)
		{
			ICacheEntry result = null;

			if (CacheMisses.TryGetValue(target, out result))
				return result;

			Type cacheMissType = typeof(CacheEntry<>).MakeGenericType(target);
			return CacheMisses[target] = (ICacheEntry)Activator.CreateInstance(cacheMissType);
		}

		protected struct CacheKey : IEquatable<CacheKey>
		{
			public readonly Type Type;
			public readonly string Name;

			public CacheKey(Type type, string name)
			{
				Type = type;
				Name = name;
			}

			public override int GetHashCode()
			{
				return Type.GetHashCode() ^ (Name != null ? Name.GetHashCode() : 0);
			}

			public override bool Equals(object obj)
			{
				if (obj == null || !(obj is CacheKey))
					return false;
				return Equals((CacheKey)obj);
			}

			public bool Equals(CacheKey other)
			{
				return Type == other.Type && Name == other.Name;
			}

			public static bool operator ==(CacheKey left, CacheKey right)
			{
				return left.Type == right.Type && left.Name == right.Name;
			}

			public static bool operator !=(CacheKey left, CacheKey right)
			{
				return left.Type != right.Type || left.Name != right.Name;
			}
		}

		protected internal interface ICacheEntry
		{
			Func<object> StaticFactory { get; }
			Func<IRezolverContainer, object> DynamicFactory { get; }
		}

		protected class CacheEntry<TTarget> : ICacheEntry
		{
			public bool IsMiss { get; private set; }
			public static readonly CacheEntry<TTarget> Miss = new CacheEntry<TTarget>();

			public Func<object> StaticFactory { get; private set; }
			public Func<IRezolverContainer, object> DynamicFactory { get; private set; }
			public Func<TTarget> StrongStaticFactory { get; private set; }
			public Func<IRezolverContainer, TTarget> StrongDynamicFactory { get; private set; }

			///// <summary>
			///// Used only to create a cache miss - all of the faactories exposed by this entry will 
			///// throw an exception if executed.
			///// </summary>
			public CacheEntry()
			{
				//TODO: Make cache entry delegates in CacheMiss type throw exceptions so upstream callers don't have
				//to check for null/_isMiss - they can simply call the delegates.
				StaticFactory = () => ResolveFailure(typeof(TTarget));
				DynamicFactory = (container) => ResolveFailureDynamic(typeof(TTarget), container);
				StrongStaticFactory = ResolveFailure;
				StrongDynamicFactory = ResolveFailureDynamic;
				IsMiss = true;
			}

			public CacheEntry(Func<TTarget> strongStaticFactory, Func<IRezolverContainer, TTarget> strongDynamicFactory,
				Func<object> staticFactory, Func<IRezolverContainer, object> dynamicFactory)
			{
				StaticFactory = staticFactory;
				DynamicFactory = dynamicFactory;
				StrongStaticFactory = strongStaticFactory;
				StrongDynamicFactory = strongDynamicFactory;
			}

			public static CacheEntry<TTarget> CreateMiss()
			{
				return new CacheEntry<TTarget>()
				{
					StaticFactory = () => ResolveFailure(typeof(TTarget)),
					DynamicFactory = (container) => ResolveFailureDynamic(typeof(TTarget), container),
					StrongStaticFactory = ResolveFailure,
					StrongDynamicFactory = ResolveFailureDynamic,
					IsMiss = true
				};
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

			private static TTarget ResolveFailure()
			{
				//TODO: Localise this string
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", typeof(TTarget)));
			}

			private static TTarget ResolveFailureDynamic(IRezolverContainer container)
			{
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", typeof(TTarget)));
			}
		}

		#endregion

		/// <summary>
		/// scope from which this container is built
		/// </summary>
		private readonly IRezolverScope _scope;
		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, ICacheEntry> _typeOnlyCacheEntries = new Dictionary<Type, ICacheEntry>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<CacheKey, ICacheEntry> _namedCacheEntries = new Dictionary<CacheKey, ICacheEntry>();

		private readonly IRezolveTargetCompiler _compiler;

		private IRezolveTargetCompiler Compiler
		{
			get { return _compiler; }
		}

		public RezolverContainer(IRezolverScope scope)
		{
			//TODO: check for null scope.
			_scope = scope;
			_compiler = RezolveTargetCompiler.Default;
		}

		public bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return _scope.Fetch(type, name) != null;
			//return RezolverCache.GetFactory(type, CreateFactoryFunc, name) != null;
		}

		public bool CanResolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: And again - change this to refer to the cache
			return _scope.Fetch<T>(name) != null;
		}

		public object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve(type, name))
					return dynamicContainer.Resolve(type, name);

				return ExecuteDynamicFactory(type, name, dynamicContainer);
			}

			return ExecuteStaticFactory(type, name);
		}

		private object ExecuteStaticFactory(Type type, string name)
		{
			return (name == null
				? GetCacheEntry(type)
				: GetCacheEntry(new CacheKey(type, name))).StaticFactory();
		}

		private object ExecuteDynamicFactory(Type type, string name, IRezolverContainer dynamicContainer)
		{
			return (name == null
				? GetCacheEntry(type)
				: GetCacheEntry(new CacheKey(type, name))).DynamicFactory(dynamicContainer);
		}

		public T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve<T>(name))
					return dynamicContainer.Resolve<T>(name);

				return ExecuteStrongDynamicFactory<T>(name, dynamicContainer);
			}

			return ExecuteStrongStaticFactory<T>(name);
		}

		private T ExecuteStrongStaticFactory<T>(string name)
		{
			return ((CacheEntry<T>) (name == null
				? GetCacheEntry(typeof (T))
				: GetCacheEntry(new CacheKey(typeof (T), name)))).StrongStaticFactory();
		}

		private T ExecuteStrongDynamicFactory<T>(string name, IRezolverContainer dynamicContainer)
		{
			return ((CacheEntry<T>) (name == null
				? GetCacheEntry(typeof (T))
				: GetCacheEntry(new CacheKey(typeof (T), name)))).StrongDynamicFactory(dynamicContainer);
		}

		public void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			//you are not allowed to register targets directly into a container
			throw new NotSupportedException();
		}

		public IRezolveTarget Fetch(Type type, string name = null)
		{
			return _scope.Fetch(type, name);
		}

		public IRezolveTarget Fetch<T>(string name = null)
		{
			return _scope.Fetch(typeof(T), name);
		}

		public INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scopee, wwe don't support the call.
			if (create) throw new NotSupportedException();

			return _scope.GetNamedScope(path, false);
		}

		#region caching implementation

		private ICacheEntry GetCacheEntry(Type type)
		{
			ICacheEntry toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
			var target = Fetch(type, null);

			if (target != null)
				return _typeOnlyCacheEntries[type] = MakeCacheEntry(type, target, this);

			return _typeOnlyCacheEntries[type] = MakeCacheMiss(type);
		}

		private ICacheEntry GetCacheEntry(CacheKey key)
		{
			ICacheEntry toReturn;
			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			var target = Fetch(key.Type, key.Name);

			if (target != null)
				return
					_namedCacheEntries[key] =
						MakeCacheEntry(key.Type, target, this);

			return _namedCacheEntries[key] = MakeCacheMiss(key.Type);
		}

		#endregion

	}
}