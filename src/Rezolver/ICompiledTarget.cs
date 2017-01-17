// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
namespace Rezolver
{
	/// <summary>
	/// Provides an abstraction for creating objects based on a given <see cref="ResolveContext"/> - this is
	/// the ultimate target of all <see cref="IContainer.Resolve(ResolveContext)"/> calls in the standard
	/// container implementations within the Rezolver framework.
	/// </summary>
	/// <remarks>In the standard implementations of <see cref="IContainer"/> (e.g. <see cref="Container"/>),
	/// a <see cref="Compilation.ITargetCompiler"/> creates instances of this from <see cref="ITarget"/>s which are 
	/// registered in an <see cref="ITargetContainer"/>.
	/// 
	/// When the container is then called upon to resolve an instance of a particular type, the <see cref="ICompiledTarget"/> is first
	/// obtained, and then the responsibility for creating the object is delegated to its <see cref="GetObject(ResolveContext)"/>
	/// method.</remarks>
	public interface ICompiledTarget
	{
		/// <summary>
		/// Called to get/create an object, potentially using the passed <paramref name="context"/> to resolve additional dependencies
		/// (via its <see cref="ResolveContext.Container"/>), or interact with any lifetime scope which is 'active' for that request
		/// (through <see cref="ResolveContext.Scope"/>).
		/// </summary>
		/// <param name="context">The current rezolve context.</param>
		/// <returns>The object that is constructed.  The return value can legitimately be null.</returns>
		/// <exception cref="InvalidOperationException">If the target fails to create the object</exception>
		/// <exception cref="Exception">Any other application-level exception could be raised by this operation</exception>
		object GetObject(ResolveContext context);
	}
}
