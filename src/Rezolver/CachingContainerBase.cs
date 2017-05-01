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
        /// <param name="targets">Optional. Contains the targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created</param>
        /// <remarks>This constructor will not use any <see cref="IContainerBehaviour"/> objects to perform post-creation initialisation
        /// of the container.  If you want it to do so, then use the <see cref="CachingContainerBase.CachingContainerBase(IContainerBehaviour, ITargetContainer)"/>
        /// constructor.</remarks>
        protected CachingContainerBase(ITargetContainer targets = null)
			: base(targets)
		{

		}

        /// <summary>
        /// Creates (and initialises) a new instance of the <see cref="CachingContainerBase"/> class.
        /// </summary>
        /// <param name="initialiser">Can be null.  An initialiser which configures the new instance (and its <see cref="Targets"/>)
        /// with additional functionality, such as compiler etc.  If not provided, then the <see cref="GlobalBehaviours.ContainerBehaviour"/>
        /// will be used.</param>
        /// <param name="targets">Optional.  Contains the targets that will be used as the source of registrations for the container.
        /// 
        /// If not provided, then a new <see cref="TargetContainer"/> will be created.</param>
        /// <remarks>To create an instance without using initialisers, use the 
        /// <see cref="CachingContainerBase.CachingContainerBase(ITargetContainer)"/> constructor.</remarks>
        protected CachingContainerBase(IContainerBehaviour initialiser, ITargetContainer targets = null)
            : base(initialiser, targets)
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

            return _entries.GetOrAdd(context.RequestedType, c => new Lazy<ICompiledTarget>(() => base.GetCompiledRezolveTarget(context))).Value;
		}
	}
}