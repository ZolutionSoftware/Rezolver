// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
	/// Builds on the <see cref="ContainerBase"/> abstract class to introduce thread-safe caching of compiled targets
	/// so they are only compiled once.
	/// </summary>
	public abstract class CachingContainerBase : ContainerBase
	{
		private readonly ConcurrentDictionary<Type, Lazy<ICompiledTarget>> _entries = new ConcurrentDictionary<Type, Lazy<ICompiledTarget>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CachingContainerBase"/> class.
		/// </summary>
		protected CachingContainerBase()
		{

		}

		protected override ICompiledTarget GetCompiledRezolveTarget(RezolveContext context)
		{
				return _entries.GetOrAdd(context.RequestedType, t => new Lazy<ICompiledTarget>(() => base.GetCompiledRezolveTarget(context))).Value;
		}
	}
}