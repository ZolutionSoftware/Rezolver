﻿namespace Rezolver.Compilation
{
	/// <summary>
	/// Interface for an object which creates <see cref="ICompileContext"/> instances for when the system needs to
	/// compile an <see cref="ITarget"/> into an <see cref="ICompiledTarget"/>.
	/// </summary>
	/// <remarks>In normal operation, this interface is closely related to the <see cref="ITargetCompiler"/> interface 
	/// because the core <see cref="ContainerBase"/> class (which provides most of the default implementation for 
	/// <see cref="IContainer"/>) obtains a new <see cref="ICompileContext"/> by resolving an instance of this interface, and 
	/// calling the <see cref="CreateContext(ResolveContext, ITargetContainer, IContainer)"/> method, passing the result
	/// to the <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> method, along with the target to be compiled.
	/// 
	/// Frequently, implementations of <c>ITargetCompiler</c> will also implement this interface to ensure that the compilation
	/// context contains everything the compiler needs to do its work.
	/// </remarks>
	public interface ICompileContextProvider
	{
		/// <summary>
		/// Creates a compilation context for the given <paramref name="resolveContext"/> - which is used to determine
		/// the <see cref="ResolveContext.RequestedType"/> that the eventual <see cref="ICompiledTarget"/> should return.
		/// </summary>
		/// <param name="resolveContext">The resolve context - used to get the <see cref="ResolveContext.RequestedType"/>
		/// and the <see cref="ResolveContext.Container"/> (if <paramref name="containerOverride"/> is not provided).</param>
		/// <param name="targets">The target container that should be used to lookup other non-compiled targets.</param>
		/// <param name="containerOverride">The container requesting the new compilation context, if different from the
		/// <see cref="ResolveContext.Container"/> on the <paramref name="resolveContext"/></param>
		ICompileContext CreateContext(ResolveContext resolveContext, ITargetContainer targets, IContainer containerOverride = null);
	}
}