using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Rezolver
{
	public abstract class RezolverBase : IRezolver
	{
		private static readonly
			Dictionary<Type, ICompiledRezolveTarget>
			MissingTargets = new Dictionary<Type, ICompiledRezolveTarget>();

		private static ICompiledRezolveTarget GetMissingTarget(Type target)
		{
			ICompiledRezolveTarget result = null;

			if (MissingTargets.TryGetValue(target, out result))
				return result;

			return MissingTargets[target] = new MissingCompiledTarget(target);
		}

		protected struct RezolverKey : IEquatable<RezolverKey>
		{
			public readonly Type Type;
			public readonly string Name;

			public RezolverKey(Type type, string name)
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
				if (obj == null || !(obj is RezolverKey))
					return false;
				return Equals((RezolverKey)obj);
			}

			public bool Equals(RezolverKey other)
			{
				return Type == other.Type && Name == other.Name;
			}

			public static bool operator ==(RezolverKey left, RezolverKey right)
			{
				return left.Type == right.Type && left.Name == right.Name;
			}

			public static bool operator !=(RezolverKey left, RezolverKey right)
			{
				return left.Type != right.Type || left.Name != right.Name;
			}
		}

		protected class MissingCompiledTarget : ICompiledRezolveTarget
		{
			private readonly Type _type;

			public MissingCompiledTarget(Type type)
			{
				_type = type;
			}

			public object GetObject()
			{
				throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
			}

			public object GetObjectDynamic(IRezolver @dynamic)
			{
				throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
			}
		}

		protected RezolverBase()
		{
		}

		//protected RezolverBase(IRezolverBuilder Builder, IRezolveTargetCompiler compiler = null)
		//{
		//	_builder = Builder;
		//	_compiler = compiler;
		//}

		public abstract IRezolveTargetCompiler Compiler { get; }

		protected abstract IRezolverBuilder Builder { get; }

		public virtual object Resolve(Type type, string name = null, IRezolver @dynamic = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamic != null)
			{
				if (dynamic.CanResolve(type, name))
					return dynamic.Resolve(type, name);

				//let's try it without the type only cache
				return (name == null ?
					 GetCompiledRezolveTarget(type)
					 : GetCompiledRezolveTarget(new RezolverKey(type, name))).GetObjectDynamic(dynamic);
			}

			return (name == null ?
					 GetCompiledRezolveTarget(type)
					 : GetCompiledRezolveTarget(new RezolverKey(type, name))).GetObject();
		}

		public virtual T Resolve<T>(string name = null, IRezolver @dynamic = null)
		{
			//I actually wonder whether this should chain up to the dynamic container on the caller's
			//behalf or not - the caller could just do it themselves.
			if (dynamic != null)
			{
				if (dynamic.CanResolve<T>(name))
					return dynamic.Resolve<T>(name);

				return
					(T)GetCompiledRezolveTarget(new RezolverKey(typeof(T), name)).GetObjectDynamic(
						dynamic);
			}

			return (T)GetCompiledRezolveTarget(new RezolverKey(typeof(T), name)).GetObject();
		}

		public virtual ILifetimeRezolver CreateLifetimeContainer()
		{
			return new LifetimeRezolver(this);
		}

		public virtual bool CanResolve(Type type, string name = null, IRezolver @dynamic = null)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Builder.Fetch(type, name) != null;
		}

		public virtual bool CanResolve<T>(string name = null, IRezolver @dynamic = null)
		{
			//TODO: And again - change this to refer to the cache
			return Builder.Fetch<T>(name) != null;
		}

		public virtual void Register(IRezolveTarget target, Type type = null, RezolverScopePath path = null)
		{
			//you are not allowed to register targets directly into a container by default
			throw new NotSupportedException();
		}

		public virtual IRezolveTarget Fetch(Type type, string name = null)
		{
			return Builder.Fetch(type, name);
		}

		public virtual IRezolveTarget Fetch<T>(string name = null)
		{
			return Builder.Fetch(typeof(T), name);
		}

		public virtual INamedRezolverBuilder GetNamedScope(RezolverScopePath path, bool create = false)
		{
			//if the caller potentially wants a new named Builder, we don't support the call.
			if (create) throw new NotSupportedException();

			return Builder.GetNamedScope(path, false);
		}

		protected virtual ICompiledRezolveTarget GetCompiledRezolveTarget(RezolverKey key)
		{
			ICompiledRezolveTarget toReturn;
			var target = Fetch(key.Type, key.Name);

			if (target != null)
				return Compiler.CompileTarget(target, this, ExpressionHelper.DynamicRezolverParam, null);

			return GetMissingTarget(key.Type);
		}

		protected virtual ICompiledRezolveTarget GetCompiledRezolveTarget(Type type)
		{
			ICompiledRezolveTarget toReturn;
			
			var target = Fetch(type);

			if (target != null)
				return Compiler.CompileTarget(target, this, ExpressionHelper.DynamicRezolverParam, null);

			return GetMissingTarget(type);
		}
	}

	public abstract class CachingRezolver : RezolverBase
	{
		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, ICompiledRezolveTarget> _typeOnlyCacheEntries = new Dictionary<Type, ICompiledRezolveTarget>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<RezolverKey, ICompiledRezolveTarget> _namedCacheEntries = new Dictionary<CachingRezolver.RezolverKey, ICompiledRezolveTarget>();

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolverKey key)
		{
			ICompiledRezolveTarget toReturn;
			if (_namedCacheEntries.TryGetValue(key, out toReturn))
				return toReturn;
			return _namedCacheEntries[key] = base.GetCompiledRezolveTarget(key);
		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(Type type)
		{
			ICompiledRezolveTarget toReturn;
			if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
				return toReturn;
			return _typeOnlyCacheEntries[type] = base.GetCompiledRezolveTarget(type);
		}
	}

	/// <summary>
	/// </summary>
	public class Rezolver : CachingRezolver
	{
		public Rezolver(IRezolverBuilder builder, IRezolveTargetCompiler compiler = null)
		{
			_builder = builder;
			_compiler = compiler;
		}

		private IRezolveTargetCompiler _compiler;
		public override IRezolveTargetCompiler Compiler
		{
			get { return _compiler; }
		}

		private IRezolverBuilder _builder;
		protected override IRezolverBuilder Builder
		{
			get { return _builder; }
		}

	}
}