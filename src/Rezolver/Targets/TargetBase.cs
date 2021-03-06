﻿// Copyright (c) Zolution Software Ltd. All rights reserved.
// Licensed under the MIT License, see LICENSE.txt in the solution root for license information


using System;
using System.Threading;

namespace Rezolver.Targets
{
    /// <summary>
    /// Abstract base class, suggested as a starting point for implementations of <see cref="ITarget"/>.
    /// </summary>
    public abstract class TargetBase : ITarget
    {
        private static int _id = 1;

        /// <summary>
        /// Gets the next available ID for a target.
        /// </summary>
        /// <remarks>
        /// This function is not generally intended to be called from your code.
        /// </remarks>
        /// <returns>The next application-unique ID available for a new target</returns>
        public static int NextId()
        {
            return Interlocked.Increment(ref _id);
        }

        /// <summary>
        /// Implementation of <see cref="ITarget.Id"/>.  Unique Id for this target.
        ///
        /// Always initialised to a new <see cref="Guid"/> using <see cref="Guid.NewGuid"/>
        /// </summary>
        public int Id { get; private set; }

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
            if(type == null) throw new ArgumentNullException(nameof(type));
            // removed generic type test here because it's a blunt instrument.
            return TypeHelpers.AreCompatible(DeclaredType, type);
        }

        /// <summary>
        /// Returns a string similar to <c>"&lt;[TargetType], DeclaredType=[DeclaredType]&gt;"</c>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"<{GetType().Name}, DeclaredType={DeclaredType}>";
        }

        /// <summary>
        /// Default constructor for derived types
        /// </summary>
        protected TargetBase()
            : this(NextId())
        {

        }

        /// <summary>
        /// Can be used by derived types to initialise the base with a specific ID.  Specifically used by targets which 'proxy' others,
        /// such as with the <see cref="VariantMatchTarget"/> etc.
        /// </summary>
        /// <param name="id"></param>
        protected TargetBase(int id)
        {
            Id = id;
        }
    }
}
