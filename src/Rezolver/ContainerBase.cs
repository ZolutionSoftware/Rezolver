// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	public abstract class ContainerBase : IContainer
	{
		private static readonly
			ConcurrentDictionary<Type, Lazy<ICompiledTarget>> MissingTargets = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();

		protected static ICompiledTarget GetMissingTarget(Type target)
		{
			return MissingTargets.GetOrAdd(target, t => new Lazy<ICompiledTarget>(() => new MissingCompiledTarget(t))).Value;
		}

		protected static bool IsMissingTarget(ICompiledTarget target)
		{
			return target is MissingCompiledTarget;
		}

		protected class MissingCompiledTarget : ICompiledTarget
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

		protected ContainerBase()
		{
		}

		public abstract ITargetCompiler Compiler { get; }

		public abstract ITargetContainer Builder { get; }

		public virtual object Resolve(RezolveContext context)
		{
			return GetCompiledRezolveTarget(context).GetObject(context);
		}

		public virtual bool TryResolve(RezolveContext context, out object result)
		{
			var target = GetCompiledRezolveTarget(context);
			if (!IsMissingTarget(target))
			{
				result = target.GetObject(context);
				return true;
			}
			else
			{
				result = null;
				return false;
			}
		}

		public virtual IScopedContainer CreateLifetimeScope()
		{
			//optimistic implementation of this method - attempts a safe cast to ILifetimeScopeRezolver of itself
			//so that types derived from this class that are ILifetimeScopeRezolver instances do not need
			//to reimplement this method.
			return new OverridingScopedContainer(this as IScopedContainer, this);
		}

		public virtual ICompiledTarget FetchCompiled(RezolveContext context)
		{
			//note that this rezolver is fixed as the rezolver in the compile context - regardless of the
			//one passed in.  This is important.
			//note also that this rezolver is only passed as scope if the context doesn't already have one.
			return GetCompiledRezolveTarget(context.CreateNew(this, context.Scope ?? (this as IScopedContainer)));
		}

		public virtual bool CanResolve(RezolveContext context)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Builder.Fetch(context.RequestedType) != null;
		}

		protected virtual ICompiledTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			ITarget target = Builder.Fetch(context.RequestedType);

			if (target == null)
				return GetFallbackCompiledRezolveTarget(context);

			//if the entry advises us to fall back if possible, then we'll see what we get from the 
			//fallback operation.  If it's NOT the missing target, then we'll use that instead
			if (target.UseFallback)
			{
				var fallback = GetFallbackCompiledRezolveTarget(context);
				if (!IsMissingTarget(fallback))
					return fallback;
			}


			//note that if a name was passed we're grabbing the best matching named builder to use for resolving
			//dependencies.
			return Compiler.CompileTarget(target,
				new CompileContext(this,
					context.RequestedType));

		}

		protected virtual ICompiledTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return GetMissingTarget(context.RequestedType);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return GetService(serviceType);
		}

		/// <summary>
		/// protected virtual implementation of IServiceProvider.GetService.
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		protected virtual object GetService(Type serviceType)
		{
			//IServiceProvider should return null if not found - so we use TryResolve.
			object toReturn = null;
			this.TryResolve(serviceType, out toReturn);
			return toReturn;
		}
	}
}
