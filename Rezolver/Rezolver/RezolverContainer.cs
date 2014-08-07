using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
	public abstract class RezolverContainerBase : IRezolverContainer
	{
		protected RezolverContainerBase()
		{
		}

		protected RezolverContainerBase(IRezolverScope scope, IRezolveTargetCompiler compiler = null)
		{
			_scope = scope;
			_compiler = compiler;
		}

		private IRezolveTargetCompiler _compiler;
		public virtual IRezolveTargetCompiler Compiler
		{
			get { return _compiler; }
			protected set
			{
				if (_compiler != null)
					throw new InvalidOperationException("Once the compiler is set, it cannot be changed.");
				_compiler = value;
			}
		}

		private IRezolverScope _scope;
		protected virtual IRezolverScope Scope
		{
			get { return _scope; }
			set
			{
				if (_scope != null)
					throw new InvalidOperationException("Once the scope has been set, it cannot be changed.");
				_scope = value;
			}
		}

		public abstract object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null);

		public abstract T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null);

		public abstract IRezolverContainer CreateLifetimeContainer();

		public virtual bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Scope.Fetch(type, name) != null;
		}

		public virtual bool CanResolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//TODO: And again - change this to refer to the cache
			return Scope.Fetch<T>(name) != null;
		}

		public virtual void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			//you are not allowed to register targets directly into a container
			throw new NotSupportedException();
		}

		public virtual IRezolveTarget Fetch(Type type, string name = null)
		{
			return Scope.Fetch(type, name);
		}

		public virtual IRezolveTarget Fetch<T>(string name = null)
		{
			return Scope.Fetch(typeof(T), name);
		}

		public virtual INamedRezolverScope GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named scope, we don't support the call.
			if (create) throw new NotSupportedException();

			return Scope.GetNamedScope(path, false);
		}
	}

	/// <summary>
	/// </summary>
	public class RezolverContainer : RezolverContainerBase
	{
		#region nested cache types

		private static readonly
			Dictionary<Type, ICompiledRezolveTarget>
			CacheMisses = new Dictionary<Type, ICompiledRezolveTarget>();

		private static ICompiledRezolveTarget MakeCacheMiss(Type target)
		{
			ICompiledRezolveTarget result = null;

			if (CacheMisses.TryGetValue(target, out result))
				return result;

			return CacheMisses[target] = new MissingCompiledTarget(target);
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

		private class MissingCompiledTarget/*<TTarget>*/ : ICompiledRezolveTarget/*<TTarget>*/
		{
			private readonly Type _type;

			public MissingCompiledTarget(Type type)
			{
				_type = type;
			}

			public object GetObject()
			{
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", _type));
			}

			public object GetObjectDynamic(IRezolverContainer dynamicContainer)
			{
				throw new InvalidOperationException(string.Format("Could not resolve type {0}", _type));
			}
		}

		#endregion

		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, ICompiledRezolveTarget> _typeOnlyCacheEntries = new Dictionary<Type, ICompiledRezolveTarget>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<CacheKey, ICompiledRezolveTarget> _namedCacheEntries = new Dictionary<CacheKey, ICompiledRezolveTarget>();
		

		public RezolverContainer(IRezolverScope scope, IRezolveTargetCompiler compiler = null)
			: base(scope, compiler)
		{
			
		}

		public override object Resolve(Type type, string name = null, IRezolverContainer dynamicContainer = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve(type, name))
					return dynamicContainer.Resolve(type, name);

				//let's try it without the type only cache
				return (name == null ?
					 GetCompiledRezolveTarget(type)
					 : GetCompiledRezolveTarget(new CacheKey(type, name))).GetObjectDynamic(dynamicContainer); 
			}

			return (name == null ?
					 GetCompiledRezolveTarget(type)
					 : GetCompiledRezolveTarget(new CacheKey(type, name))).GetObject(); 
		}

		private ICompiledRezolveTarget GetCompiledRezolveTarget(CacheKey key)
		{
			ICompiledRezolveTarget toReturn;
			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			var target = Fetch(key.Type, key.Name);

			if (target != null)
				return _namedCacheEntries[key] = Compiler.CompileTarget(target, this, ExpressionHelper.DynamicContainerParam, null);

			return _namedCacheEntries[key] = MakeCacheMiss(key.Type);
		}

		private ICompiledRezolveTarget GetCompiledRezolveTarget(Type type)
		{
			ICompiledRezolveTarget toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
				var target = Fetch(type);

			if (target != null)
				return _typeOnlyCacheEntries[type] = Compiler.CompileTarget(target, this, ExpressionHelper.DynamicContainerParam, null);

			return _typeOnlyCacheEntries[type] = MakeCacheMiss(type);
		}

		public override T Resolve<T>(string name = null, IRezolverContainer dynamicContainer = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamicContainer != null)
			{
				if (dynamicContainer.CanResolve<T>(name))
					return dynamicContainer.Resolve<T>(name);

				return
					(T)GetCompiledRezolveTarget(new CacheKey(typeof (T), name)).GetObjectDynamic(
						dynamicContainer);
			}

			return (T)GetCompiledRezolveTarget(new CacheKey(typeof(T), name)).GetObject();
		}

		public override IRezolverContainer CreateLifetimeContainer()
		{
			throw new NotImplementedException();
		}
	}
}