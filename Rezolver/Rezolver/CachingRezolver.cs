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
		//private readonly Dictionary<Type, ICompiledRezolveTarget> _typeOnlyCacheEntries = new Dictionary<Type, ICompiledRezolveTarget>();
		/// <summary>
		/// This cache is for factories resolved by type and name
		/// </summary>
		//private readonly Dictionary<RezolverKey, ICompiledRezolveTarget> _namedCacheEntries = new Dictionary<RezolverKey, ICompiledRezolveTarget>();

		private readonly Dictionary<RezolveContext, ICompiledRezolveTarget> _entries = new Dictionary<RezolveContext, ICompiledRezolveTarget>();

		protected CachingRezolver()
		{

		}

		protected CachingRezolver(bool enableDynamicRezolvers = DefaultEnableDynamicRezolvers)
			: base(enableDynamicRezolvers)
		{

		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			ICompiledRezolveTarget toReturn;
			if (_entries.TryGetValue(context, out toReturn))
				return toReturn;
			//create a new context to use as the key which doesn't hold on to any dynamic rezolver
			//or lifetime scope.
			var keyContext = new RezolveContext(context.RequestedType, context.Name);
			return _entries[context] = base.GetCompiledRezolveTarget(context);
		}

		//protected override ICompiledRezolveTarget GetCompiledRezolveTarget(Type type)
		//{
		//	ICompiledRezolveTarget toReturn;
		//	if (_typeOnlyCacheEntries.TryGetValue(type, out toReturn))
		//		return toReturn;
		//	return _typeOnlyCacheEntries[type] = base.GetCompiledRezolveTarget(type);
		//}
	}
}