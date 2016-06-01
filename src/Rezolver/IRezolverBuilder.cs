using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rezolver
{
	public interface IRezolverBuilder
	{
		/// <summary>
		/// Allows a caller to introspect all the registrations that have been added to this builder.
		/// </summary>
		IEnumerable<KeyValuePair<RezolveContext, IRezolveTarget>> AllRegistrations { get; }
		/// <summary>
		/// Registers a target, optionally for a particular target type.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="serviceType">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="IRezolveTarget.DeclaredType"/> of the <paramref name="target"/></param>
		void Register(IRezolveTarget target, Type serviceType = null);
		/// <summary>
		/// Searches for the target for a particular type
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <returns></returns>
		IRezolveTargetEntry Fetch(Type type);		
	}
}
