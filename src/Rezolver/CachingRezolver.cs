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
		private readonly ConcurrentDictionary<RezolveContext, Lazy<ICompiledRezolveTarget>> _entries = new ConcurrentDictionary<RezolveContext, Lazy<ICompiledRezolveTarget>>();
		private readonly ConcurrentDictionary<Type, Lazy<ICompiledRezolveTarget>> _typeOnlyEntries = new ConcurrentDictionary<Type, Lazy<ICompiledRezolveTarget>>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CachingRezolver"/> class.
		/// </summary>
		protected CachingRezolver()
		{

		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			if (context.Name == null)
			{
				return _typeOnlyEntries.GetOrAdd(context.RequestedType, t => new Lazy<ICompiledRezolveTarget>(() => base.GetCompiledRezolveTarget(context))).Value;

			}
			else
			{
				Lazy<ICompiledRezolveTarget> lazy;
				if (_entries.TryGetValue(context, out lazy))
					return lazy.Value;

				return _entries.GetOrAdd(new RezolveContext(null, context.RequestedType, context.Name),
						c => new Lazy<ICompiledRezolveTarget>(() => base.GetCompiledRezolveTarget(c))).Value;
			}
		}
	}
}