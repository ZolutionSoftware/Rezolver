// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Rezolver.Compilation;

namespace Rezolver
{
	/// <summary>
	/// Builds on the <see cref="ContainerBase"/> base class to introduce thread-safe caching of compiled targets so they are only compiled once per requested type.
	/// 
	/// Only creatable through inheritance.
	/// </summary>
	/// <remarks>Internally, the class uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> to store <see cref="ICompiledTarget"/>s keyed by the requested type.
	/// 
	/// All the main <see cref="IContainer"/> implementations used directly in an application should ideally inherit from this class, because otherwise every 
	/// <see cref="IContainer.Resolve(IResolveContext)"/> operation would require a compilation phase before the object could be returned, which would be incredibly slow.
	/// 
	/// It's because of this caching that registering new targets in any <see cref="ITargetContainer"/> used by this class is not recommended: because after the first request
	/// for a particular type is made, the resultant <see cref="ICompiledTarget"/> is fixed until the container is thrown away.</remarks>
	public class CachingContainerBase : ContainerBase
	{
        private readonly ConcurrentDictionary<Type, Lazy<ICompiledTarget>> _entries
            = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();
        /// <summary>
        /// Creates a new instance of the <see cref="CachingContainerBase"/> class.
        /// </summary>
        /// <param name="targets">Optional.  The target container whose registrations will be used for dependency lookup when <see cref="Resolve(IResolveContext)"/> 
        /// (and other operations) is called.  If not provided, a new <see cref="TargetContainer"/> instance is constructed.  This will ultimately be available 
        /// to inherited types, after construction, through the <see cref="Targets"/> property.</param>
        /// <remarks>This constructor does not attach any <see cref="IContainerBehaviour"/> behaviours, because behaviours typically
        /// call methods which are declared virtual on this class - which could be unsafe.
        /// 
        /// If this does not apply to your derived class (which is unlikely) - use the 
        /// <see cref="CachingContainerBase.CachingContainerBase(IContainerBehaviour, ITargetContainer)"/> constructor.</remarks>
        protected CachingContainerBase(ITargetContainer targets = null)
			: base(targets)
		{

		}

        /// <summary>
        /// Creates a new instance of the <see cref="CachingContainerBase"/> class.
        /// </summary>
        /// <param name="behaviour">Can be null.  A behaviour to attach to this container (and, potentially its <see cref="Targets"/>).
        /// If not provided, then the global <see cref="GlobalBehaviours.ContainerBehaviour"/> will be used.</param>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the container,
        /// ultimately being passed to the <see cref="Targets"/> property.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <remarks>To create an instance without attaching behaviours, use the 
        /// <see cref="CachingContainerBase.CachingContainerBase(ITargetContainer)"/> constructor.</remarks>
        protected CachingContainerBase(IContainerBehaviour behaviour, ITargetContainer targets = null)
            : base(behaviour, targets)
        {

        }

		/// <summary>
		/// Obtains an <see cref="ICompiledTarget"/> for the given <paramref name="context"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		/// <remarks>The method is called by <see cref="ContainerBase.Resolve(IResolveContext)"/>
		/// to get the compiled target whose <see cref="ICompiledTarget.GetObject(IResolveContext)"/> method is to be used to get the instance that is to be resolved for
		/// a given request.
		/// 
		/// The internal cache is examined first to see if an entry exists for the <see cref="IResolveContext.RequestedType"/> type and, if not, then 
		/// the result of the base class' <see cref="ContainerBase.GetCompiledRezolveTarget(IResolveContext)"/> is cached and returned.
		/// </remarks>
		protected override ICompiledTarget GetCompiledRezolveTarget(IResolveContext context)
		{
            if (_entries.TryGetValue(context.RequestedType, out Lazy<ICompiledTarget> myLazy))
                return myLazy.Value;

            return _entries.GetOrAdd(
                context.RequestedType, 
                c => new Lazy<ICompiledTarget>(() => base.GetCompiledRezolveTarget(context))).Value;
		}
	}
}