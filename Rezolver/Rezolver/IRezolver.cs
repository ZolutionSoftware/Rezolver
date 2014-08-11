using System;

namespace Rezolver
{
	/// <summary>
	/// The final compiled rezolver built from an IRezolverBuilder (which, in truth, represents
	/// a potential tree of builders)
	/// Note that the interface also implements the IRezolverBuilder interface in order that it
	/// can provide both concrete instances and the means by which to build them, in order to 
	/// donate expressions to other scopes/containers that might need to cross-call.
	/// </summary>
	public interface IRezolver : IRezolverBuilder
	{
		/// <summary>
		/// Provides access to the compiler used by this containerm allowing you to inherit it.
		/// </summary>
		IRezolveTargetCompiler Compiler { get; }
		/// <summary>
		/// Standard version of the CanResolve operation.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		/// <param name="dynamic"></param>
		/// <returns></returns>
		bool CanResolve(Type type, string name = null, IRezolver @dynamic = null);
		/// <summary>
		/// Generic version of the CanResolve operation
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="dynamic"></param>
		/// <returns></returns>
		bool CanResolve<T>(string name = null, IRezolver @dynamic = null);

		/// <summary>
		/// Resolves an object of the given type, optionally with the given name, using the optional
		/// dynamic container for any late-bound resolve calls.
		/// </summary>
		/// <param name="type">Required. The type of the dependency to be resolved.</param>
		/// <param name="name">Optional.  The name of the dependency to be resolved.</param>
		/// <param name="dynamic">Optional.  A dynamic Builder to be used in performing 
		///   additional resolve calls triggered from the underlying Builder's expression.  This container
		///   should not resolve aagainst this dynamic Builder directly - it is only to be used
		///   when late-inding a secondary or tertiary (ad nauseam) dependency.</param>
		/// <returns></returns>
		object Resolve(Type type, string name = null, IRezolver @dynamic = null);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="dynamic"></param>
		/// <returns></returns>
		T Resolve<T>(string name = null, IRezolver @dynamic = null);

		ILifetimeRezolver CreateLifetimeContainer();

		//TODO: I think we need this in the IRezolver interface so that we can more explictly share
		//compiled code between rezolvers.
		ICompiledRezolveTarget FetchCompiled(Type type, string name);
	}
}