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
		object Rezolve(Type type, string name = null, IRezolverScope dynamicScope = null);
	}
}