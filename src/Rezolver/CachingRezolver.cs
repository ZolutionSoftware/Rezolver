using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
	/// Builds on the RezolverBase abstract class to introduce thread-safe caching of compiled targets
	/// so they only have to be compiled once.
	/// </summary>
	public abstract class CachingRezolver : RezolverBase
	{
		private readonly ConcurrentDictionary<Type, Lazy<ICompiledRezolveTarget>> _entries = new ConcurrentDictionary<Type, Lazy<ICompiledRezolveTarget>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CachingRezolver"/> class.
		/// </summary>
		protected CachingRezolver()
		{

		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
				return _entries.GetOrAdd(context.RequestedType, t => new Lazy<ICompiledRezolveTarget>(() => base.GetCompiledRezolveTarget(context))).Value;
		}
	}
}