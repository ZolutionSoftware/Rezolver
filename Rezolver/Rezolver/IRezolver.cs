using System;

namespace Rezolver
{
	/// <summary>
	/// The final compiled rezolver built from an IRezolverBuilder
	/// Note that the interface also implements the IRezolverBuilder interface in order that it
	/// can provide both concrete instances and the means by which to build them, in order to 
	/// allow separate rezolvers to chain.  Rezolvers are not expected to implement IRezolverBuilder directly,
	/// however, they are expected to proxy the inner <see cref="Builder"/> - however consumers looking to 
	/// get registrations of a rezolver should always do it through the rezolver, in case of any special logic
	/// being applied outside of the builder itself.
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
		//bool CanResolve(Type type, string name = null, IRezolver @dynamic = null);

		bool CanResolve(RezolveContext context);

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
		object Resolve(RezolveContext context);

		/// <summary>
		/// Merges the CanResolve and Resolve operations into one call.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		bool TryResolve(RezolveContext context, out object result);

		/// <summary>
		/// Called to create a lifetime scope that will track, and dispose of, any 
		/// disposable objects that are created.
		/// </summary>
		/// <returns></returns>
		ILifetimeScopeRezolver CreateLifetimeScope();

		//TODO: I think we need this in the IRezolver interface so that we can more explictly share
		//compiled code between rezolvers.
		ICompiledRezolveTarget FetchCompiled(RezolveContext context);
	}

	//note - I've gone the overload route instead of default parameters to aid runtime optimisation
	//in the case of one or two parameters only being required.  Believe me, it does make a difference.

	public static class IRezolverExtensions
	{
		public static object Resolve(this IRezolver rezolver, Type type)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type));
		}

		public static object Resolve(this IRezolver rezolver, Type type, string name)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, type, name));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, requestedType, scope));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(rezolver, requestedType, name, scope));
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, string name, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, scope), out result);
		}

		public static bool TryResolve(this IRezolver rezolver, Type type, string name, ILifetimeScopeRezolver scope, out object result)
		{
			return rezolver.TryResolve(new RezolveContext(rezolver, type, name, scope), out result);
		}
	}
}