using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public abstract class RezolverBase : IRezolver
	{
		/// <summary>
		/// The default for wwhether dynamic rezolvers are enabled or not in any rezolvers that inherit
		/// from this base class.
		/// </summary>
		public const bool DefaultEnableDynamicRezolvers = true;

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

		protected class MissingCompiledTarget : ICompiledRezolveTarget
		{
			private readonly Type _type;

			public MissingCompiledTarget(Type type)
			{
				_type = type;
			}

			public object GetObject(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
			}
		}

		private readonly bool _enableDynamicRezolvers;

		protected RezolverBase()
			: this(DefaultEnableDynamicRezolvers)
		{
		}

		/// <summary>
		/// Use this constructor to explicitly enable or disable dynamic rezolvers
		/// in the code that is generated for this rezolver, and in this rezolver
		/// in general.
		/// </summary>
		/// <param name="enableDynamicRezolvers"></param>
		protected RezolverBase(bool enableDynamicRezolvers = DefaultEnableDynamicRezolvers)
		{
			_enableDynamicRezolvers = enableDynamicRezolvers;
		}

		public abstract IRezolveTargetCompiler Compiler { get; }

		protected abstract IRezolverBuilder Builder { get; }

		public virtual object Resolve(RezolveContext context)
		{
			if (context.DynamicRezolver != null 
				&& _enableDynamicRezolvers 
				&& context.DynamicRezolver != this)
			{
				if (context.DynamicRezolver.CanResolve(context))
					return context.DynamicRezolver.Resolve(context);
			}
			return GetCompiledRezolveTarget(context).GetObject(context);
		}

		public virtual ILifetimeScopeRezolver CreateLifetimeScope()
		{
			return new LifetimeScopeRezolver(this);
		}

		public ICompiledRezolveTarget FetchCompiled(RezolveContext context)
		{
			return GetCompiledRezolveTarget(context.CreateNew(context.Scope == null ? this as ILifetimeScopeRezolver : context.Scope));
		}

		public virtual bool CanResolve(RezolveContext context)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Builder.Fetch(context.RequestedType, context.Name) != null;
		}

		public virtual void Register(IRezolveTarget target, Type type = null, RezolverPath path = null)
		{
			//you are not allowed to register targets directly into a rezolver by default
			Builder.Register(target, type, path);
		}

		public virtual IRezolveTarget Fetch(Type type, string name = null)
		{
			return Builder.Fetch(type, name);
		}

		public virtual IRezolveTarget Fetch<T>(string name = null)
		{
			return Builder.Fetch(typeof(T), name);
		}

		public virtual INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false)
		{
			//if the caller potentially wants a new named Builder, we don't support the call.
			if (create) throw new NotSupportedException();

			return Builder.GetNamedBuilder(path, false);
		}

		protected virtual ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			IRezolveTarget target = Fetch(context.RequestedType, context.Name);
			
			if (target != null)
				return Compiler.CompileTarget(target, new CompileContext(this, context.RequestedType, enableDynamicRezolver: _enableDynamicRezolvers));

			return GetMissingTarget(context.RequestedType);
		}		
	}
}
