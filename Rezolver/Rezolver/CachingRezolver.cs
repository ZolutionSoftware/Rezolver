using System;
using System.Collections.Generic;

namespace Rezolver
{
	/// <summary>
	/// Builds on the RezolverBase abstract class to introduce (non-thread-safe) caching of compiled targets
	/// so they only have to be compiled once.
	/// </summary>
	public abstract class CachingRezolver : RezolverBase
	{
		/// <summary>
		/// This cache is for factories resolved by type only
		/// </summary>
		private readonly Dictionary<Type, ICompiledRezolveTarget> _typeOnlyCacheEntries = new Dictionary<Type, ICompiledRezolveTarget>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		private readonly Dictionary<RezolverKey, ICompiledRezolveTarget> _namedCacheEntries = new Dictionary<RezolverKey, ICompiledRezolveTarget>();

		protected CachingRezolver()
		{

		}

		protected CachingRezolver(bool enableDynamicRezolvers = DefaultEnableDynamicRezolvers)
			: base(enableDynamicRezolvers)
		{

		}

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
}