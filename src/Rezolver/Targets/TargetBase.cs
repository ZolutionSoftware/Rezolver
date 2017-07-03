// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Linq.Expressions;
using System.Reflection;
using Rezolver.Compilation;

namespace Rezolver.Targets
{
	/// <summary>
	/// Abstract base class, suggested as a starting point for implementations of <see cref="ITarget"/>.
	/// </summary>
	public abstract class TargetBase : ITarget
	{
		/// <summary>
		/// Implementation of <see cref="ITarget.UseFallback"/>
		/// 
		/// Base version always returns <c>false</c>.
		/// </summary>
		public virtual bool UseFallback
		{
			get { return false; }
		}

        /// <summary>
		/// Gets the declared type of object that is constructed by this target.
		/// </summary>
		public abstract Type DeclaredType
        {
            get;
        }

        /// <summary>
        /// Gets the scoping behaviour for instances that will ultimately be produced by this target.
        /// </summary>
        /// <value>The scope behaviour.</value>
        /// <remarks>Base implementation always returns <see cref="ScopeBehaviour.Implicit"/>.</remarks>
        public virtual ScopeBehaviour ScopeBehaviour
        {
            get
            {
                return ScopeBehaviour.Implicit;
            }
        }

        /// <summary>
        /// Get the preferred scope in which an object produced from this target should be tracked.
        /// </summary>
        /// <remarks>Base implementation always returns <see cref="ScopePreference.Current"/></remarks>
        public virtual ScopePreference ScopePreference
        {
            get
            {
                return ScopePreference.Current;
            }
        }

        /// <summary>
        /// Implementation of <see cref="ITarget.SupportsType(Type)"/>. Returns a boolean indicating whether the target 
        /// is able to produce an instance of, or an instance that is compatible with, the passed <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <remarks>It is strongly suggested that you use this method to check whether the target can construct
        /// an instance of a given type rather than performing any type checking yourself on the
        /// <see cref="DeclaredType" />, because an <see cref="ITarget" /> might be able to support a much wider
        /// range of types other than just those which are directly compatible with its <see cref="DeclaredType" />.
        /// 
        /// For example, the <see cref="GenericConstructorTarget" /> is statically bound to an open generic, so therefore
        /// traditional type checks on the <see cref="DeclaredType" /> do not work.  That class' implementation of this
        /// method, however, contains the complex logic necessary to determine if the open generic can be closed into a
        /// generic type which is compatible with the given <paramref name="type" />.
        /// 
        /// Implementations of <see cref="Compilation.ITargetCompiler" /> should always consult this function in their
        /// implementation of <see cref="Compilation.ITargetCompiler.CompileTarget(ITarget, Compilation.ICompileContext)" />
        /// to determine if the target is compatible with the <see cref="Compilation.CompileContext.TargetType" /> of the
        /// <see cref="Compilation.CompileContext" />
        /// </remarks>
        public virtual bool SupportsType(Type type)
		{
			type.MustNotBeNull("type");
			//removed generic type test here because it's a blunt instrument.
			return TypeHelpers.AreCompatible(DeclaredType, type);
		}
	}
}
