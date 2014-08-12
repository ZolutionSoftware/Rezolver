using System;

namespace Rezolver
{
	/// <summary>
	/// The final compiled rezolver built from an IRezolverBuilder
	/// Note that the interface also implements the IRezolverBuilder interface in order that it
	/// can provide both concrete instances and the means by which to build them, in order to 
	/// allow separate rezolvers to chain.
	/// </summary>
	public interface IRezolver : IRezolverBuilder
	{
		/// <summary>
		/// Provides access to the compiler used by this rezolver in turning IRezolveTargets into
		/// ICompiledRezolveTargets.
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
		/// dynamic rezolver for any late-bound resolve calls.
		/// </summary>
		/// <param name="type">Required. The type of the dependency to be resolved.</param>
		/// <param name="name">Optional.  The name of the dependency to be resolved.</param>
		/// <param name="dynamicRezolver">Optional.  If provided, then the eventual rezolved
		/// instance could either come from, or have one or more of it's dependencies come from,
		/// here instead of from this rezolver.</param>
		/// <returns></returns>
		object Resolve(Type type, string name = null, IRezolver dynamicRezolver = null);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <param name="dynamic"></param>
		/// <returns></returns>
		T Resolve<T>(string name = null, IRezolver @dynamic = null);

		/// <summary>
		/// Called to create a lifetime scope that will track, and dispose of, any 
		/// disposable objects that are created.
		/// </summary>
		/// <returns></returns>
		ILifetimeScopeRezolver CreateLifetimeScope();

		//TODO: I think we need this in the IRezolver interface so that we can more explictly share
		//compiled code between rezolvers.
		ICompiledRezolveTarget FetchCompiled(Type type, string name);
	}
}