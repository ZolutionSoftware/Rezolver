using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// The primary IOC entry point for the Rezolver framework.  This interface implies more than just
	/// an object that can resolve dependencies or locate services - for example, it is suggested that an <see cref="ITargetContainer"/> instance
	/// (see <see cref="Builder"/>) be used to store the core type registrations from which this resolver will be built.
	/// </summary>
	/// <remarks>
	/// If an implementation is indeed using the <see cref="Builder"/> to build a set of registrations from which objects will be created, 
	/// then that implementation should, also, allow for new registrations to be added to the builder throughout the lifetime of the 
	/// <see cref="IContainer"/>.
	/// 
	/// However - A caller cannot, expect to be able to resolve Type 'X',
	/// then make some modification to the builder which can causes Type 'X' to be built differently, the next time it is resolved.  Implementations
	/// of <see cref="IContainer"/> are free to treat the a dependency graph of an object of a resolved type (not necessarily the objects themselves,
	/// but the types of objects that are resolved and how they, in turn, are built) as fixed after the first resolve operation is done.
	/// </remarks>
	public interface IContainer : IServiceProvider
	{
		/// <summary>
		/// Provides access to the builder for this container - so that registrations can be added to the rezolver after
		/// construction.  It is not a requirement of a rezolver to use a builder to act as source of registrations, therefore 
		/// if a builder is not applicable to this instance, either return a stub instance that always returns notargets, or
		/// throw a NotSupportException.
		/// </summary>
		ITargetContainer Builder { get; }
		/// <summary>
		/// Provides access to the compiler used by this rezolver in turning IRezolveTargets into
		/// ICompiledRezolveTargets.
		/// </summary>
		ITargetCompiler Compiler { get; }
		/// <summary>
		/// Returns true if a resolve operation for the given context will succeed.
		/// If you're going to be calling <see cref="Resolve"/> immediately afterwards, consider using the TryResolve method instead,
		/// which allows you to check and obtain the result at the same time.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns><c>true</c> if this instance can resolve the specified context; otherwise, <c>false</c>.</returns>
		bool CanResolve(RezolveContext context);

		/// <summary>
		/// The core 'resolve' operation in Rezolver.
		/// 
		/// The object is resolved using state from the passed <paramref name="context"/> (type to be resolved, any names,
		/// lifetime scopes, and a reference to the original resolver instance that is 'in scope', which could be a different resolver to
		/// this resolver.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The resolved object, if successful, otherwise an exception should be raised.</returns>
		object Resolve(RezolveContext context);

		/// <summary>
		/// Merges the CanResolve and Resolve operations into one call.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="result">The result.</param>
		/// <returns><c>true</c> if the operation succeeded (the resolved object will be set into the <paramref name="result"/> 
		/// parameter); <c>false</c> otherwise.</returns>
		bool TryResolve(RezolveContext context, out object result);

		/// <summary>
		/// Called to create a lifetime scope that will track, and dispose of, any 
		/// disposable objects that are created via calls to <see cref="Resolve"/> (to the lifetime scope
		/// itself, not to the resolver that 'parents' the lifetime scope).
		/// </summary>
		/// <returns>An <see cref="IScopedContainer"/> instance that will use this resolver to resolve objects,
		/// but which will impose its own lifetime restrictions on those instances.</returns>
		IScopedContainer CreateLifetimeScope();
		
		/// <summary>
		/// Fetches the compiled target for the given context.
		/// 
		/// This is not typically a method that consumers of an <see cref="IContainer" /> are likely to use; it's more typically
		/// used by code generation code (or even generated code) to interoperate between two resolvers, or indeed over other object.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>ICompiledRezolveTarget.</returns>
		ICompiledTarget FetchCompiled(RezolveContext context);
	}
}