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
			return rezolver.Resolve(new RezolveContext(type));
		}

		public static object Resolve(this IRezolver rezolver, Type type, string name)
		{
			return rezolver.Resolve(new RezolveContext(type, name));
		}

		public static object Resolve(this IRezolver rezolver, Type requestedType, IRezolver dynamicRezolver)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, dynamicRezolver));
		}

		public static object Resolve(this IRezolver rezolver, Type requestedType, string name, IRezolver dynamicRezolver)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, name, dynamicRezolver));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, scope));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, string name, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, name, scope));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, IRezolver dynamicRezolver, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, dynamicRezolver, scope));
		}
		
		public static object Resolve(this IRezolver rezolver, Type requestedType, string name, IRezolver dynamicRezolver, ILifetimeScopeRezolver scope)
		{
			return rezolver.Resolve(new RezolveContext(requestedType, name, dynamicRezolver, scope));
		}
	}
}