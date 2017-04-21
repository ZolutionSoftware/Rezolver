// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


namespace Rezolver.Compilation
{
	/// <summary>
	/// Interface for an object which creates <see cref="ICompileContext"/> instances for when the system needs to
	/// compile an <see cref="ITarget"/> into an <see cref="ICompiledTarget"/>.
	/// </summary>
	/// <remarks>In normal operation, this interface is closely related to the <see cref="ITargetCompiler"/> interface 
	/// because the core <see cref="ContainerBase"/> class (which provides most of the default implementation for 
	/// <see cref="IContainer"/>) obtains a new <see cref="ICompileContext"/> by resolving an instance of this interface, and 
	/// calling the <see cref="CreateContext(IResolveContext, ITargetContainer)"/> method, passing the result
	/// to the <see cref="ITargetCompiler.CompileTarget(ITarget, ICompileContext)"/> method, along with the target to be compiled.
	/// 
	/// Frequently, implementations of <c>ITargetCompiler</c> will also implement this interface to ensure that the compilation
	/// context contains everything the compiler needs to do its work.
	/// </remarks>
	public interface ICompileContextProvider
	{
		/// <summary>
		/// Creates a compilation context for the given <paramref name="resolveContext"/> - which is used to determine
		/// the <see cref="IResolveContext.RequestedType"/> that the eventual <see cref="ICompiledTarget"/> should return.
		/// </summary>
		/// <param name="resolveContext">The resolve context - used to get the <see cref="IResolveContext.RequestedType"/>
		/// and the <see cref="IResolveContext.Container"/>, and will be set on the <see cref="ICompileContext.ResolveContext"/>
        /// property of the returned context.</param>
		/// <param name="targets">The target container that should be used to lookup other non-compiled targets.</param>
		ICompileContext CreateContext(IResolveContext resolveContext, ITargetContainer targets);
	}
}