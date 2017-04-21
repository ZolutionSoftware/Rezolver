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
		// developer note: This used to be a c/current dictionary of lazy objects, but benchmarking and performance analysis showed
		// that it was slower than simply allowing redundant compiled targets to be created and have one thead 'win'.
		//private readonly ConcurrentDictionary<IResolveContext, Lazy<ICompiledTarget>> _entries 
		//	= new ConcurrentDictionary<IResolveContext, Lazy<ICompiledTarget>>(ResolveContext.RequestedTypeComparer);
        private readonly ConcurrentDictionary<Type, Lazy<ICompiledTarget>> _entries
            = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();
        /// <summary>
        /// Initializes a new instance of the <see cref="CachingContainerBase"/> class.
        /// </summary>
        /// <param name="targets">Optional. Contains the targets that will be used to create the <see cref="ICompiledTarget"/>s that this container will use to produce objects
        /// when requested.
        /// 
        /// If not provided, then the base class' default (see <see cref="ContainerBase.ContainerBase(ITargetContainer, IContainerBehaviour)"/>) will be used.</param>
        /// <param name="compilerConfig">Optional.  An object which will be used to configure this container and its targets to use a specific compilation
        /// strategy.  If <c>null</c>, then the <see cref="CompilerConfiguration.DefaultProvider"/> provider will be used.</param>
        protected CachingContainerBase(ITargetContainer targets = null, IContainerBehaviour compilerConfig = null)
			: base(targets, compilerConfig)
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