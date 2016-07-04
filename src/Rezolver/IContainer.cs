// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// A container through which instances of objects can be <see cref="Resolve"/>d.  
	/// </summary>
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
		/// The core 'resolve' operation for a Rezolver container.
		/// 
		/// The object is resolved using state from the passed <paramref name="context"/>, including any active lifetime scope and 
		/// a reference to the original container instance that was called, which could be a different container to this one.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>The resolved object, if successful.</returns>
    /// <exception cref="System.InvalidOperationException">If the requested type cannot be resolved.</exception>
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