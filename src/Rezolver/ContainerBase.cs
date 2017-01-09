// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rezolver
{
	/// <summary>
	/// Starting point for implementations of <see cref="IContainer"/> - only creatable through inheritance.
	/// </summary>
	/// <remarks>This class also implements <see cref="ITargetContainer"/> by proxying the <see cref="Targets"/> that are
	/// provided to it on construction (or created anew if not supplied).  All of those interface methods are implemented 
	/// explicitly except the <see cref="Register(ITarget, Type)"/> method,  which is available through the class' public API.
	/// 
	/// Note: <see cref="IContainer"/>s are generally not expected to implement <see cref="ITargetContainer"/>, and the framework
	/// will never assume they do.
	/// 
	/// The reason this class does is to make it easier to create a new container and to register targets into it without having to worry about
	/// managing a separate <see cref="ITargetContainer"/> instance in your application root - because all the registration extension methods defined
	/// in <see cref="SingletonTargetDictionaryExtensions"/> will be available to developers in code which has a reference to this class, or one derived from it.
	/// 
	/// Note also that calling <see cref="ITargetContainer.CombineWith(ITargetContainer, Type)"/> on an instance of this type will always
	/// cause a <see cref="NotSupportedException"/> to be thrown.
	/// </remarks>
	public class ContainerBase : IContainer, ITargetContainer
	{
		private static readonly
		  ConcurrentDictionary<Type, Lazy<ICompiledTarget>> MissingTargets = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();

		/// <summary>
		/// Gets an <see cref="ICompiledTarget"/> for the given type which will always throw an <see cref="InvalidOperationException"/> whenever its
		/// <see cref="ICompiledTarget.GetObject(RezolveContext)"/> method is called.  Use this when you can't resolve a target for a type.
		/// </summary>
		/// <param name="type">The type for which you wish to create a missing target.</param>
		protected static ICompiledTarget GetMissingTarget(Type type)
		{
			return MissingTargets.GetOrAdd(type, t => new Lazy<ICompiledTarget>(() => new MissingCompiledTarget(t))).Value;
		}

		/// <summary>
		/// Determines whether the given <paramref name="target"/> is an instance of <see cref="MissingCompiledTarget"/>.
		/// </summary>
		/// <param name="target">The target.</param>
		protected static bool IsMissingTarget(ICompiledTarget target)
		{
			return target is MissingCompiledTarget;
		}

		/// <summary>
		/// Used as a sentinel type when a type cannot be resolved by a <see cref="ContainerBase"/> instance.  Instead of returning a null
		/// <see cref="ICompiledTarget"/> instance, the container will construct an instance of this type (typically through <see cref="GetMissingTarget(Type)"/>,
		/// which caches singleton instances of this class on a per-type basis) which can then be used just as if the lookup succeeded.
		/// </summary>
		/// <seealso cref="Rezolver.ICompiledTarget" />
		protected class MissingCompiledTarget : ICompiledTarget
		{
			private readonly Type _type;

			public MissingCompiledTarget(Type type)
			{
				_type = type;
			}

			/// <summary>
			/// Implementation of <see cref="ICompiledTarget.GetObject(RezolveContext)"/>.  Always throws an <see cref="InvalidOperationException"/>.
			/// </summary>
			/// <param name="context">The current rezolve context.</param>
			/// <exception cref="InvalidOperationException">Always thrown.</exception>
			public object GetObject(RezolveContext context)
			{
				throw new InvalidOperationException(String.Format("Could not resolve type {0}", _type));
			}
		}

		/// <summary>
		/// Constructs a new instance of the <see cref="ContainerBase"/>, optionally initialising it with the given <paramref name="targets"/> and <paramref name="compiler"/>
		/// </summary>
		/// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when <see cref="Resolve(RezolveContext)"/> (and other operations)
		/// is called.  If not provided, a new <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available to inherited types, after construction, through the 
		/// <see cref="Targets"/> property.</param>
		/// <param name="compiler">Optional.  The compiler which will be used to create <see cref="ICompiledTarget"/> instances from the <see cref="ITarget"/> instances which 
		/// are registered in the <paramref name="targets"/> target container during resolve-time.  If not provided, then the <see cref="TargetCompiler.Default"/> compiler is
		/// used.</param>
		protected ContainerBase(ITargetContainer targets = null, ITargetCompiler compiler = null)
		{
			Targets = targets ?? new TargetContainer();
			Compiler = compiler ?? TargetCompiler.Default;
			//register the current default target compiler as our default compiler.
			Register(TargetCompiler.Default.AsObjectTarget());
		}

		/// <summary>
		/// The compiler that will be used to compile <see cref="ITarget"/> instances (obtained from the <see cref="Targets"/> container
		/// during <see cref="Resolve(RezolveContext)"/> and <see cref="TryResolve(RezolveContext, out object)"/> operations) into 
		/// <see cref="ICompiledTarget"/> instances that will actually provide the objects that are resolved.
		/// </summary>
		/// <remarks>Notes to implementers: This property must NEVER be null.</remarks>
		protected ITargetCompiler Compiler { get; }

		/// <summary>
		/// Provides the <see cref="ITarget"/> instances that will be compiled by the <see cref="Compiler"/> into <see cref="ICompiledTarget"/>
		/// instances.
		/// </summary>
		/// <remarks>Notes to implementers: This property must NEVER be null.
		/// 
		/// This class implements the <see cref="ITargetContainer"/> interface by wrapping around this instance so that an application can create 
		/// an instance of <see cref="ContainerBase"/> and directly register targets into it; rather than having to create and setup the target container
		/// first.
		/// 
		/// You can add registrations to this target container at any point in the lifetime of any <see cref="ContainerBase"/> instances which are attached
		/// to it.  In reality, however, if any <see cref="Resolve(RezolveContext)"/> operations have been performed prior to adding more registrations,
		/// then there's no guarantee that new dependencies will be picked up - especially if the <see cref="CachingContainerBase"/> is being used as your
		/// application's container (which it nearly always will be).</remarks>
		protected ITargetContainer Targets { get; }

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
			//note that this container is fixed as the container in the compile context - regardless of the
			//one passed in.  This is important.
			//note also that this container is only passed as scope if the context doesn't already have one.
			return GetCompiledRezolveTarget(context.CreateNew(this, context.Scope ?? (this as IScopedContainer)));
		}

		public virtual bool CanResolve(RezolveContext context)
		{
			//TODO: Change this to refer to the cache (once I've figured out how to do it based on the new compiler)
			return Targets.Fetch(context.RequestedType) != null;
		}

		protected virtual ICompiledTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			ITarget target = Targets.Fetch(context.RequestedType);

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

			//if the target also supports the ICompiledTarget interface then return it.
			if (target is ICompiledTarget)
				return (ICompiledTarget)target;

			//if ITargetCompiler has been requested then we can't continue, because in order
			//to compile this target we'd need a compiler we can use which, by definition, we don't have.
			//also, the same error occurs if the container in question is unable to resolve the type.
			//note that this could trigger this same line of code to blow in the context container.
			var compilerContext = context.CreateNew(typeof(IContainer));
			ITargetCompiler compiler;
			if(context.RequestedType == typeof(ITargetCompiler) || !context.Container.TryResolve(out compiler))
				throw new InvalidOperationException("The compiler has not been correctly configured for this container.  It must be registered in the container's targets and must be a target that supports the ICompiledTarget interface.");
			
			return compiler.CompileTarget(target,
			  new CompileContext(this,
				//the targets we pass here are wrapped in a new ChildBuilder by the context
				Targets,
				context.RequestedType));

		}

		/// <summary>
		/// Called by <see cref="GetCompiledRezolveTarget(RezolveContext)"/> if no valid <see cref="ITarget"/> can be
		/// found for the <paramref name="context"/> or if the one found has its <see cref="ITarget.UseFallback"/> property
		/// set to <c>true</c>.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>An <see cref="ICompiledTarget"/> to be used as the result of a <see cref="Resolve(RezolveContext)"/> operation
		/// where the search for a valid target either fails or is inconclusive (e.g. - empty enumerables).</returns>
		/// <remarks>The base implementation always returns an instance of the <see cref="MissingCompiledTarget"/> via
		/// the <see cref="GetMissingTarget(Type)"/> static method.</remarks>
		protected virtual ICompiledTarget GetFallbackCompiledRezolveTarget(RezolveContext context)
		{
			return GetMissingTarget(context.RequestedType);
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			return GetService(serviceType);
		}

		/// <summary>
		/// Protected virtual implementation of <see cref="IServiceProvider.GetService(Type)"/>.
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

		/// <summary>
		/// Implementation of <see cref="ITargetContainer.Register(ITarget, Type)"/> - simply proxies the
		/// call to the target container referenced by the <see cref="Targets"/> property.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="serviceType"></param>
		/// <remarks>Remember: registering new targets into an <see cref="ITargetContainer"/> after an <see cref="IContainer"/>
		/// has started compiling targets within it can yield unpredictable results.
		/// 
		/// If you create a new container and perform all your registrations before you use it, however, then everything will 
		/// work as expected.
		/// 
		/// Note also the other ITargetContainer interface methods are implemented explicitly so as to hide them from the 
		/// list of class members.
		/// </remarks>
		public void Register(ITarget target, Type serviceType = null)
		{
			Targets.Register(target, serviceType);
		}

		#region ITargetContainer explicit implementation
		ITarget ITargetContainer.Fetch(Type type)
		{
			return Targets.Fetch(type);
		}

		IEnumerable<ITarget> ITargetContainer.FetchAll(Type type)
		{
			return Targets.FetchAll(type);
		}

		ITargetContainer ITargetContainer.CombineWith(ITargetContainer existing, Type type)
		{
			throw new NotSupportedException();
		}
		#endregion
	}
}
