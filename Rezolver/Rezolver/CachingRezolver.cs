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
		private readonly Dictionary<RezolveContext, ICompiledRezolveTarget> _entries = new Dictionary<RezolveContext, ICompiledRezolveTarget>();
		private readonly Dictionary<Type, ICompiledRezolveTarget> _typeOnlyEntries = new Dictionary<Type, ICompiledRezolveTarget>();

		protected CachingRezolver()
		{

		}

		protected override ICompiledRezolveTarget GetCompiledRezolveTarget(RezolveContext context)
		{
			ICompiledRezolveTarget toReturn;
			if (context.Name == null)
			{
				if (_typeOnlyEntries.TryGetValue(context.RequestedType, out toReturn))
					return toReturn;

				return _typeOnlyEntries[context.RequestedType] = base.GetCompiledRezolveTarget(context);
			}
			else
			{
				if (_entries.TryGetValue(context, out toReturn))
					return toReturn;
				//create a new context to use as the key which doesn't hold on to any dynamic rezolver
				//or lifetime scope.
				var keyContext = new RezolveContext(null, context.RequestedType, context.Name);
				return _entries[context] = base.GetCompiledRezolveTarget(context);
			}
		}
	}
}