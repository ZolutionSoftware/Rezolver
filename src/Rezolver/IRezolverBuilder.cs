using Rezolver.Resources;
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
		/// Registers a target, optionally for a particular target type and optionally
		/// under a particular name.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="serviceType">Optional.  The type the target is to be registered against, if different
		/// from the <see cref="IRezolveTarget.DeclaredType"/> of the <paramref name="target"/></param>
		/// <param name="path">Optional.  The path under which this target is to be registered.  One or more
		/// new named rezolvers could be created to accommodate the registration.</param>
		void Register(IRezolveTarget target, Type serviceType = null, RezolverPath path = null);
		/// <summary>
		/// Searches for the target for a particular type and optionally
		/// under a particular name/path.
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <param name="name">Optional.  The named builder to be searched.</param>
		/// <returns></returns>
		IRezolveTargetEntry Fetch(Type type, string name = null);		

		/// <summary>
		/// Retrieves, after optionally creating, a named Builder from this Builder.
		/// </summary>
		/// <param name="path">Required.  The path of the Builder to be retrieved or created.</param>
		/// <param name="create">If the Builder(s) do/does not exist, this parameter is used to specify whether you
		///   want it/them to be created.</param>
		/// <returns>Null if no Builder is found.  Otherwise the Builder that was found or created.</returns>
		INamedRezolverBuilder GetNamedBuilder(RezolverPath path, bool create = false);
		/// <summary>
		/// Searches for the best-matched named child builder, or none if not applicable.
		/// 
		/// No creation on the fly is supported, obviously.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		INamedRezolverBuilder GetBestNamedBuilder(RezolverPath path);
	}
}
