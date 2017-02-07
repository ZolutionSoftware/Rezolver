// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using Rezolver.Targets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Rezolver
{
	/// <summary>
	/// Represents an action to be performed in order to obtain an object to satisfy a call to <see cref="IContainer.Resolve(ResolveContext)"/>,
	/// when the standard implementations of <see cref="IContainer"/> are used (e.g. <see cref="Container"/>).
	/// </summary>
	/// <remarks>
	/// As mentioned in the summary, the role of this interface is largely determined by the framework's own standard implementations of the 
	/// <see cref="IContainer"/> interface - all of which use an <see cref="ITargetContainer"/> to store service registrations which, when
	/// <see cref="IContainer.Resolve(ResolveContext)"/> is called, is queried to obtain one or more targets which will have been registered
	/// for the requested type.
	/// 
	/// After obtaining a target, an <see cref="Compilation.ITargetCompiler"/> is then used to compile the target(s) into an
	/// <see cref="ICompiledTarget"/> whose <see cref="ICompiledTarget.GetObject(ResolveContext)"/> method will ultimately then be called to
	/// 'resolve' the instance.  The role of the target, then, is to act as a description of the action that is to be performed by that
	/// compiled target that is built from it.
	/// 
	/// The interface doesn't describe the type of target in hand - it only provides the core data required to query the static type of
	/// the target (the type of object that the target will produce) and to determine compatibility with a request for a particular type.
	/// 
	/// 
	/// 
	/// The framework's many implementations of this interface - e.g. <see cref="ConstructorTarget"/>, <see cref="SingletonTarget"/>,
	/// <see cref="ResolvedTarget"/> plus many others - then define the behaviour and any additional data required in order for a compiler
	/// to produce an <see cref="ICompiledTarget"/> which matches the target's intent.  E.g, the <see cref="ConstructorTarget"/>, which
	/// represents creating a new instance via a constructor, provides all the necessary information to bind to the correct constructor
	/// (including parameter bindings etc) - and the compiler's job is to translate that into an <see cref="ICompiledTarget"/> which 
	/// executes that constructor, returning the result.
	/// </remarks>
	public interface ITarget
	{
		/// <summary>
		/// If <c>true</c>, then the consumer should consider falling back to a more suitable target if available, as
		/// the object produced from this target is most likely a default of some kind - e.g. empty enumerable, default
		/// parameter value.
		/// </summary>
		bool UseFallback { get; }
		/// <summary>
		/// Gets the static type of the object produced from this target.  For example, if this target represents executing
		/// a constructor on a type, then this property should equal the type to which that constructor belongs.
		/// </summary>
		Type DeclaredType { get; }
		/// <summary>
		/// Returns a boolean indicating whether the target is able to produce an instance of, or an instance
		/// that is compatible with, the passed <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns><c>true</c> if <paramref name="type"/> is supported, <c>false</c> otherwise.</returns>
		/// <remarks>It is strongly suggested that you use this method to check whether the target can construct
		/// an instance of a given type rather than performing any type checking yourself on the 
		/// <see cref="DeclaredType"/>, because an <see cref="ITarget"/> might be able to support a much wider
		/// range of types other than just those which are directly compatible with its <see cref="DeclaredType"/>.
		/// 
		/// For example, the <see cref="GenericConstructorTarget"/> is statically bound to an open generic, so therefore
		/// traditional type checks on the <see cref="DeclaredType"/> do not work.  That class' implementation of this
		/// method, however, contains the complex logic necessary to determine if the open generic can be closed into a
		/// generic type which is compatible with the given <paramref name="type"/>.
		/// 
		/// Implementations of <see cref="Compilation.ITargetCompiler"/> should always consult this function in their
		/// implementation of <see cref="Compilation.ITargetCompiler.CompileTarget(ITarget, Compilation.ICompileContext)"/>
		/// to determine if the target is compatible with the <see cref="Compilation.CompileContext.TargetType"/> of the
		/// <see cref="Compilation.CompileContext"/>
		/// 
		/// Please note that any <paramref name="type"/> that's a generic type definition will always yield a false result,
		/// because it's impossible to build an instance of an open generic type.
		/// </remarks>
		bool SupportsType(Type type);
	}
}
