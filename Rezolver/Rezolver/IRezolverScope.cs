using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rezolver.Tests;

namespace Rezolver
{
	public interface IRezolverScope
	{
		/// <summary>
		/// Registers a target, optionally for a particular target type and optionally
		/// under a particular name.
		/// </summary>
		/// <param name="target">Required.  The target to be registereed</param>
		/// <param name="type">Optional.  The type thee target is to be registered against, if different
		/// from the declared type on the <paramref name="target"/></param>
		/// <param name="name">Optional.  The name under which this target is to be registered.  One or more
		/// new named scopes could be created to accommodate the registration.</param>
		void Register(IRezolveTarget target, Type type = null, string name = null);
		/// <summary>
		/// Searches for a target for a particular type and optionally
		/// under a particular named scope (or scopes).
		/// </summary>
		/// <param name="type">Required.  The type to be searched.</param>
		/// <param name="name">Optional.  The named scope or scopes to be searched.</param>
		/// <returns></returns>
		IRezolveTarget Fetch(Type type, string name = null);
		/// <summary>
		/// Retrieves, after optionally creating, a named scope from this scope.
		/// </summary>
		/// <param name="name">Required.  The name of the scope to be retrieved or created.</param>
		/// <param name="create">If the scope(s) do/does not exist, this parameter is used to specify whether you
		/// want it/them to be created.</param>
		/// <returns>Null if no scope is found.  Otherwise the scope that was found or created.</returns>
		INamedRezolverScope GetNamedScope(string name, bool create = false);
	}
}
