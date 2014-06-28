using System;

namespace Rezolver
{
	/// <summary>
	/// The final compiled container built from an IRezolverScope (which, in truth, represents
	/// a potential tree of scopes)
	/// Note that the interface also implements the IRezolverScope interface in order that the container
	/// can provide both concrete instances, in order to donate expressions to other scopes/containers
	/// that might need to cross-call.
	/// </summary>
	public interface IRezolverContainer : IRezolverScope
	{
		/// <summary>
		/// Standard version of the CanResolve operation.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="dynamicContainer"></param>
		/// <returns></returns>
		bool CanResolve(Type type, string name = null, IRezolverContainer dynamicContainer = null);
		/// <summary>
		/// Generic version of the CanResolve operation
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="dynamicContainer"></param>
		/// <returns></returns>
		bool CanResolve<T>(string name = null, IRezolverContainer dynamicContainer = null);
		/// <summary>
		/// Resolves an object of the given type, optionally with the given name, using the optional
		/// dynamic container for any late-bound resolve calls.
		/// </summary>
		/// <param name="type">Required. The type of the dependency to be resolved.</param>
		/// <param name="name">Optional.  The name of the dependency to be resolved.</param>
		/// <param name="dynamicContainer">Optional.  A dynamic scope to be used in performing 
		/// additional resolve calls triggered from the underlying scope's expression.  This container
		/// should not resolve aagainst this dynamic scope directly - it is only to be used
		/// when late-inding a secondary or tertiary (ad nauseam) dependency.</param>
		/// <returns></returns>
		object Rezolve(Type type, string name = null, IRezolverContainer dynamicContainer = null);
	}
}